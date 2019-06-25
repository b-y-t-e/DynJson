using DynJson.Helpers;
using DynJson.Executor;
using DynJson.Parser;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DynJson.Tokens;
using DynJson.Helpers.CoreHelpers;
using DynJson.Database;
using DynJson.Helpers.DatabaseHelpers;
using System.Diagnostics;
using Jint;
using Jint.Runtime.Interop;

namespace DynJson.Functions
{
    public class JsFunction : S4JStateFunction
    {
        public JsFunction() :
            this("js")
        {
            ReturnExactValue = true;
        }

        public JsFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new JsBrackets();
            CommentDefinition = new JsComment();
            QuotationDefinition = new JsQuotation();
            Evaluator = new JsEvaluator();
        }
    }

    public class JsFitFunction : S4JStateFunction
    {
        public JsFitFunction() :
            this("js-fit")
        {
            ReturnExactValue = false;
        }

        public JsFitFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new JsBrackets();
            CommentDefinition = new JsComment();
            QuotationDefinition = new JsQuotation();
            Evaluator = new JsEvaluator();
        }
    }

    public class JsSingleFunction : S4JStateFunction
    {
        public JsSingleFunction() :
            this("js-single")
        {
            ReturnExactValue = true;
            ReturnSingleObject = true;
        }

        public JsSingleFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new JsBrackets();
            CommentDefinition = new JsComment();
            QuotationDefinition = new JsQuotation();
            Evaluator = new JsEvaluator();
        }
    }

    public class JsManyFunction : S4JStateFunction
    {
        public JsManyFunction() :
            this("js-many")
        {
            ReturnExactValue = true;
            ReturnManyObjects = true;
        }

        public JsManyFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new JsBrackets();
            CommentDefinition = new JsComment();
            QuotationDefinition = new JsQuotation();
            Evaluator = new JsEvaluator();
        }
    }

    public class JsValueFunction : S4JStateFunction
    {
        public JsValueFunction() :
            this("js-value")
        {
            ReturnExactValue = true;
            ReturnSingleValue = true;
        }

        public JsValueFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new JsBrackets();
            CommentDefinition = new JsComment();
            QuotationDefinition = new JsQuotation();
            Evaluator = new JsEvaluator();
        }
    }

    public class JsComment : S4JState
    {
        public JsComment()
        {
            Priority = 1;
            StateType = EStateType.FUNCTION_COMMENT;
            AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_COMMENT
                };
            //IsComment = true;
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

    public class JsQuotation : S4JState
    {
        public JsQuotation()
        {
            Priority = 2;
            StateType = EStateType.FUNCTION_QUOTATION;
            //IsValue = true;
            //IsQuotation = true;
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

    public class JsBrackets : S4JState
    {
        public JsBrackets()
        {
            Priority = 3;
            StateType = EStateType.FUNCTION_BRACKETS;
            AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_BRACKETS,
                    EStateType.FUNCTION_COMMENT
                };
            //IsValue = true;
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

    public class JsEvaluatorGlobals
    {
        public dynamic Globals { get; set; }

        public JsEvaluatorGlobals()
        {
            Globals = new ExpandoObject();
        }
    }

    /*public class JsProgramCache
    {
        Dictionary<string, JsProgram> cache =
            new Dictionary<string, JsProgram>();

        public void Save(string code, JsProgram program)
        {
            lock (cache)
                cache[code] = program;
        }

        public JsProgram Get(string code)
        {
            JsProgram program = null;
            lock (cache)
                cache.TryGetValue(code, out program);
            return program;
        }
    }*/

    public class JsEvaluator : IEvaluator
    {
        /*static JsProgramCache cache =
            new JsProgramCache();*/

        public async Task<Object> Evaluate(S4JExecutor Executor, S4JToken token, IDictionary<String, object> variables)
        {
            S4JTokenFunction function = token as S4JTokenFunction;
            StringBuilder code = new StringBuilder();

            JsEvaluatorGlobals globals = new JsEvaluatorGlobals();
            IDictionary<string, object> globalVariables = globals.Globals as IDictionary<string, object>;


            /* globalVariables["list"] = (Func<List<Object>>)(() =>
             {
                 return new List<Object>();
             });

             globalVariables["dictionary"] = (Func<Dictionary<String, Object>>)(() =>
             {
                 return new Dictionary<String, Object>();
             });*/

            var engine = new Engine(cfg => cfg.AllowClr());

            engine.SetValue("list", TypeReference.CreateTypeReference(engine, typeof(List<Object>)));
            engine.SetValue("dictionary", TypeReference.CreateTypeReference(engine, typeof(Dictionary<String, Object>)));

            globalVariables["db"] = (Func<Object, DbApi>)((Parameter) =>
            {
                string sourceName = UniConvert.ToString(Parameter);
                string connectionString = !string.IsNullOrEmpty(sourceName) ?
                    Executor.Sources.Get(sourceName) :
                    Executor.Sources.GetDefault();

                DbApi api = new DbApi(connectionString, sourceName ?? Executor.Sources.DefaultSourceName);
                return api;
            });

            globalVariables["api"] = (Func<DynJsonApi>)(() =>
            {
                DynJsonApi api = new DynJsonApi(Executor);
                return api;
            });

            globalVariables["sqlbuilder"] = (Func<SqlBuilder>)(() =>
            {
                SqlBuilder sql = new SqlBuilder();
                return sql;
            });

            foreach (KeyValuePair<string, object> keyAndVal in variables)
            {
                globalVariables[keyAndVal.Key] = keyAndVal.Value;
            }

            // code.Append("function DynJsonRootFunction() { ").Append(function.ToJsonWithoutGate()).Append(" } DynJsonRootFunctionResult = DynJsonRootFunction(); ");
            code.Append(function.ToJsonWithoutGate());

            //JsProgram program = cache.Get(code.ToString());
            /*if (program == null)
            {
                Stopwatch stCompile = Stopwatch.StartNew();
                try
                {
                    program = new Engine().
                        Compile(code.ToString());

                    //cache.Save(code.ToString(), program);
                }
                finally
                {
                    if (Logger.IsEnabled)
                        Logger.LogPerformance("DYNLAN", "compile", stCompile.ElapsedMilliseconds, code.ToString());
                }
            }*/

            Stopwatch st = Stopwatch.StartNew();
            try
            {
                //engine.SetValue("DynJsonRootFunctionResult", (object)null);
                foreach (var var in globalVariables)
                    engine.SetValue(var.Key, var.Value);
                engine.Execute(code.ToString());
                object result = engine.GetCompletionValue().ToObject();
                //object result = engine.GetValue("DynJsonRootFunctionResult");

                return result;
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("DYNLAN", "eval", ex.Message, code.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("DYNLAN", "eval", st.ElapsedMilliseconds, code.ToString());
            }
        }
    }

}
