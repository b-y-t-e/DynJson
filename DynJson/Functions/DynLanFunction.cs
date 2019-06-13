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
using DynLan;
using DynLan.Classes;
using DynJson.Helpers.CoreHelpers;
using DynJson.Database;
using DynLan.Exceptions;
using DynJson.Helpers.DatabaseHelpers;
using DynLan.OnpEngine.Models;
using System.Diagnostics;

namespace DynJson.Functions
{
    public class DynLanFunction : S4JStateFunction
    {
        public DynLanFunction() :
            this("@;x")
        {
            ReturnExactValue = true;
        }

        public DynLanFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

    public class DynLanFitFunction : S4JStateFunction
    {
        public DynLanFitFunction() :
            this("@-fit;x-fit")
        {
            ReturnExactValue = false;
        }

        public DynLanFitFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

    public class DynLanSingleFunction : S4JStateFunction
    {
        public DynLanSingleFunction() :
            this("@-single;x-single")
        {
            ReturnExactValue = true;
            ReturnSingleObject = true;
        }

        public DynLanSingleFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

    public class DynLanManyFunction : S4JStateFunction
    {
        public DynLanManyFunction() :
            this("@-many;x-many")
        {
            ReturnExactValue = true;
            ReturnManyObjects = true;
        }

        public DynLanManyFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new DynLanBrackets();
            CommentDefinition = new DynLanComment();
            QuotationDefinition = new DynLanQuotation();
            Evaluator = new DynLanEvaluator();
        }
    }

    public class DynLanValueFunction : S4JStateFunction
    {
        public DynLanValueFunction() :
            this("@-value;x-value")
        {
            ReturnExactValue = true;
            ReturnSingleValue = true;
        }

        public DynLanValueFunction(string aliasName) :
            base(aliasName)
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

    public class DynLanQuotation : S4JState
    {
        public DynLanQuotation()
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

    public class DynLanBrackets : S4JState
    {
        public DynLanBrackets()
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

    public class DynLanEvaluatorGlobals
    {
        public dynamic Globals { get; set; }

        public DynLanEvaluatorGlobals()
        {
            Globals = new ExpandoObject();
        }
    }

    public class DynLanProgramCache
    {
        Dictionary<string, DynLanProgram> cache =
            new Dictionary<string, DynLanProgram>();

        public void Save(string code, DynLanProgram program)
        {
            lock (cache)
                cache[code] = program;
        }

        public DynLanProgram Get(string code)
        {
            DynLanProgram program = null;
            lock (cache)
                cache.TryGetValue(code, out program);
            return program;
        }
    }

    public class DynLanEvaluator : IEvaluator
    {
        static DynLanProgramCache cache =
            new DynLanProgramCache();

        public async Task<Object> Evaluate(S4JExecutor Executor, S4JToken token, IDictionary<String, object> variables)
        {
            S4JTokenFunction function = token as S4JTokenFunction;
            StringBuilder code = new StringBuilder();

            DynLanEvaluatorGlobals globals = new DynLanEvaluatorGlobals();
            IDictionary<string, object> globalVariables = globals.Globals as IDictionary<string, object>;

            globalVariables["db"] = new DynMethod((Parameters) =>
            {
                string sourceName = UniConvert.ToString(Parameters?.FirstOrDefault());
                string connectionString = !string.IsNullOrEmpty(sourceName) ?
                    Executor.Sources.Get(sourceName) :
                    Executor.Sources.GetDefault();

                DbApi api = new DbApi(connectionString, sourceName ?? Executor.Sources.DefaultSourceName);
                return api;
            });

            globalVariables["api"] = new DynMethod((Parameters) =>
            {
                DynJsonApi api = new DynJsonApi(Executor);
                return api;
            });

            globalVariables["sqlbuilder"] = new DynMethod((Parameters) =>
            {
                SqlBuilder sql = new SqlBuilder();
                return sql;
            });

            foreach (KeyValuePair<string, object> keyAndVal in variables)
            {
                globalVariables[keyAndVal.Key] = keyAndVal.Value;
            }

            code.Append(function.ToJsonWithoutGate());

            DynLanProgram program = cache.Get(code.ToString());
            if (program == null)
            {
                Stopwatch stCompile = Stopwatch.StartNew();
                try
                {
                    program = new Compiler().
                        Compile(code.ToString());

                    cache.Save(code.ToString(), program);
                }
                finally
                {
                    if (Logger.IsEnabled)
                        Logger.LogPerformance("DYNLAN", "compile", stCompile.ElapsedMilliseconds, code.ToString());
                }
            }

            Stopwatch st = Stopwatch.StartNew();
            try
            {
                Object result = program.
                    Eval(globalVariables);

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
