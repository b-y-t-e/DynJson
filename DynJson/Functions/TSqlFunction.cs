﻿using DynJson.Helpers;
using DynJson.Executor;
using DynJson.Parser;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using DynJson.Helpers.CoreHelpers;
using DynJson.Helpers.DatabaseHelpers;
using DynJson.Tokens;

namespace DynJson.Functions
{
    public class TSqlFunction : S4JStateFunction
    {
        public TSqlFunction() :
            this("sql")
        {
        }

        public TSqlFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new TSqlBrackets();
            CommentDefinition = new TSqlComment();
            QuotationDefinition = new TSqlQuotation();
            Evaluator = new TSqlEvaluator();
            FunctionTagExecutor = context =>
            {
                /*if (context.Tags.ContainsKey("temporary"))
                {
                    
                }*/
            };
        }
    }

    public class TSqlComment : S4JState
    {
        public TSqlComment()
        {
            Priority = 1;
            StateType = EStateType.FUNCTION_COMMENT;
            AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_COMMENT
                };
            IsComment = true;
            Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "/*".ToCharArray(),
                        End = "*/".ToCharArray()
                    },
                    new S4JStateGate()
                    {
                        Start = "--".ToCharArray(),
                        End = "\n".ToCharArray()
                    }
                };
        }
    }

    public class TSqlQuotation : S4JState
    {
        public TSqlQuotation()
        {
            Priority = 2;
            StateType = EStateType.FUNCTION_QUOTATION;
            IsValue = true;
            IsQuotation = true;
            Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "'".ToCharArray(),
                        End = "'".ToCharArray(),
                        Inner = "'".ToCharArray(),
                    }
                };
        }
    }

    public class TSqlBrackets : S4JState
    {
        public TSqlBrackets()
        {
            IsValue = true;
            StateType = EStateType.FUNCTION_BRACKETS;
            AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_BRACKETS,
                    EStateType.FUNCTION_COMMENT
                };
            Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "(".ToCharArray(),
                        End = ")".ToCharArray()
                    }
                };
        }
    }

    public class TSqlEvaluator : IEvaluator
    {
        public async Task<Object> Evaluate(
            S4JExecutor Executor,
            S4JToken token,
            IDictionary<String, object> variables)
        {
            S4JTokenFunction functionToken = token as S4JTokenFunction;
            S4JStateFunction functionState = token.State as S4JStateFunction;

            MyQuery query = new MyQuery();

            foreach (KeyValuePair<string, object> keyAndVal in variables)
            {
                BuildScriptForVariable(query, keyAndVal.Key, keyAndVal.Value);
            }

            query.Append(functionToken.ToJsonWithoutGate());

            String connectionString = Executor.Sources.Get(functionState.FunctionNames.First());
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                var result = con.SelectItems(query.ToString());
                return result;
            }
        }

        void BuildScriptForVariable(
            MyQuery Query,
            String Name,
            Object Value,
            String ParentName = null)
        {
            string name = string.IsNullOrEmpty(ParentName) ? Name : (ParentName + "_" + Name);

            if (MyTypeHelper.IsPrimitive(Value))
            {
                Query.Append($"declare @{name} {TSqlHelper.GetSqlType(Value?.GetType())};\n");
                Query.Append($"set @{name} = {{0}};\n", Value);
            }

            else if (Value is IDictionary<string, object> dict)
            {
                if (string.IsNullOrEmpty(ParentName))
                {
                    foreach (var keyAndValue in dict)
                    {
                        BuildScriptForVariable(Query, keyAndValue.Key, keyAndValue.Value, Name);
                    }
                }
                else
                {
                    BuildCreateTableScriptForVariable(Query, name, dict);
                    BuildInsertIntoTableScriptForVariable(Query, name, dict);
                }
                //BuildCreateTableScriptForVariable(Query, name, dict);
                // BuildInsertIntoTableScriptForVariable(Query, name, dict);
            }

            else if (Value is IList list)
            {
                var firstValue = ReflectionHelper.ToDictionary(list.Count > 0 ? list[0] : null);
                BuildCreateTableScriptForVariable(Query, name, firstValue);
                foreach (var item in list)
                    BuildInsertIntoTableScriptForVariable(Query, name, item);
            }

            else if (Value.GetType().IsClass)
            {
                var classAsDict = ReflectionHelper.ToDictionary(Value);
                if (string.IsNullOrEmpty(ParentName))
                {
                    foreach (var keyAndValue in classAsDict)
                    {
                        BuildScriptForVariable(Query, keyAndValue.Key, keyAndValue.Value, Name);
                    }
                }
                else
                {
                    BuildCreateTableScriptForVariable(Query, name, classAsDict);
                    BuildInsertIntoTableScriptForVariable(Query, name, classAsDict);
                }
            }
        }

        void BuildCreateTableScriptForVariable(
            MyQuery Query,
            String TableName,
            Object Value)
        {
            if (MyTypeHelper.IsPrimitive(Value))
            {

            }

            else if (Value is IDictionary<string, object> dict)
            {
                Query.Append("declare @").Append(TableName).Append(" table (");
                Int32 index = 0;
                foreach (var keyAndValue in dict)
                {
                    if (index > 0) Query.Append(", ");
                    Query.Append(keyAndValue.Key).Append(" ").Append(TSqlHelper.GetSqlType(keyAndValue.Value?.GetType()));
                    index++;
                }
                Query.Append(");");
            }

            else if (Value is IList)
            {

            }

            else if (Value.GetType().IsClass)
            {
                BuildCreateTableScriptForVariable(Query, TableName, ReflectionHelper.ToDictionary(Value));
            }
        }

        void BuildInsertIntoTableScriptForVariable(
            MyQuery Query,
            String TableName,
            Object Value)
        {
            if (MyTypeHelper.IsPrimitive(Value))
            {

            }

            else if (Value is IDictionary<string, object> dict)
            {
                Query.Append("insert into @").Append(TableName).Append(" (");
                Int32 index = 0;
                foreach (var keyAndValue in dict)
                {
                    if (index > 0) Query.Append(", ");
                    Query.Append(keyAndValue.Key);
                    index++;
                }
                Query.Append(") values (");
                index = 0;
                foreach (var keyAndValue in dict)
                {
                    if (index > 0) Query.Append(", ");
                    Query.AppendVal(keyAndValue.Value);
                    index++;
                }
                Query.Append(");");
            }

            else if (Value is IList)
            {

            }

            else if (Value.GetType().IsClass)
            {
                BuildInsertIntoTableScriptForVariable(Query, TableName, ReflectionHelper.ToDictionary(Value));
            }
        }

    }

    public static class TSqlHelper
    {
        public static string GetSqlType(Type csType)
        {
            if (csType != null)
            {
                if (MyTypeHelper.IsInteger(csType))
                    return "int";

                else if (MyTypeHelper.IsNumeric(csType))
                    return "real";

                else if (MyTypeHelper.IsDateTime(csType))
                    return "datetime";

                else if (MyTypeHelper.IsTimeSpan(csType))
                    return "datetime";
            }

            return "nvarchar(max)";
        }
    }
}
