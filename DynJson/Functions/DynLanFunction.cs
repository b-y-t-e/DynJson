using DynJson.Helpers;
using DynJson.Executor;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynJson.Tokens;
using DynLan;
using DynLan.Classes;
using DynJson.Helpers.CoreHelpers;
using DynJson.Database;

namespace DynJson.Functions
{
    public class DynLanFunction : S4JStateFunction
    {
        public DynLanFunction() :
            this("dynlan")
        {

        }

        public DynLanFunction(params string [] aliases) :
            base(aliases)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

    public class DynLanComment : S4JState
    {
        public DynLanComment()
        {
            Priority = 1;
            StateType = EStateType.FUNCTION_COMMENT;
            AllowedStateTypes = new [] 
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
                        Start = "#".ToCharArray(),
                        End = "\n".ToCharArray()
                    }
                };
        }
    }

    public class DynLanQuotation : S4JState
    {
        public DynLanQuotation()
        {
            Priority = 2;
            StateType = EStateType.FUNCTION_QUOTATION;
            IsValue = true;
            IsQuotation = true;
            Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = "\"".ToCharArray(),
                        End = "\"".ToCharArray(),
                        Inner = "\\".ToCharArray(),
                    },
                    new S4JStateGate()
                    {
                        Start = "'".ToCharArray(),
                        End = "'".ToCharArray(),
                        Inner = "\\".ToCharArray(),
                    }
                };
        }
    }

    public class DynLanBrackets : S4JState
    {
        public DynLanBrackets()
        {
            Priority = 3;
            StateType = EStateType.FUNCTION_BRACKETS;
            AllowedStateTypes = new [] 
                {
                    EStateType.FUNCTION_BRACKETS,
                    EStateType.FUNCTION_COMMENT
                };
            IsValue = true;
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

    public class DynLanEvaluatorGlobals
    {
        public dynamic Globals { get; set; }

        public DynLanEvaluatorGlobals()
        {
            Globals = new ExpandoObject();
        }
    }

    public class DynLanEvaluator : IEvaluator
    {
        public async Task<Object> Evaluate(S4JExecutor Executor, S4JToken token, IDictionary<String, object> variables)
        {
            S4JTokenFunction function = token as S4JTokenFunction;
            StringBuilder code = new StringBuilder();

            DynLanEvaluatorGlobals globals = new DynLanEvaluatorGlobals();
            IDictionary<string, object> globaVariables = globals.Globals as IDictionary<string, object>;
            // var globalObject = new Dictionary<string, object>();

            foreach (KeyValuePair<string, object> keyAndVal in variables)
            {
                globaVariables[keyAndVal.Key/*.ToUpper()*/] = keyAndVal.Value;
                /*if (keyAndVal.Value == null)
                {
                    code.Append($"object {keyAndVal.Key} = {keyAndVal.Value.SerializeJson()};\n");
                }
                else
                {
                    code.Append($"var {keyAndVal.Key} = {keyAndVal.Value.SerializeJson()};\n");
                }*/
            }

            Dictionary<String, Object> dbProxy = new Dictionary<String, Object>();
            foreach (var source in Executor.Sources)
                dbProxy[source.Key] = new DbApi(source.Value);
            globaVariables["db"] = dbProxy;

            code.Append(function.ToJsonWithoutGate());

            // string finalCode = MyStringHelper.AddReturnStatement(code.ToString());

            Object result = new Compiler().
                Compile(code.ToString()).
                Eval(globaVariables);

            return result;
        }
    }

}
