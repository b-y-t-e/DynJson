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
using System.Globalization;

namespace DynJson.Functions
{
    public class JsFunction : S4JStateFunction
    {
        public JsFunction() :
            this("js")
        {

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

    public static class JsApiDefault
    {
        static Object lck = new Object();
        static JsApi i;
        public static JsApi Get()
        {
            if (i == null)
                lock (lck)
                    if (i == null)
                    {
                        i = new JsApi();

                        JsApiDefault.Get().AddType("Guid", typeof(Guid));
                        JsApiDefault.Get().AddType("DateTime", typeof(DateTime));
                        JsApiDefault.Get().AddType("List", typeof(List<Object>));
                        JsApiDefault.Get().AddType("Dictionary", typeof(Dictionary<String, Object>));
                        JsApiDefault.Get().AddDelegate("api", (Func<DynJsonApi>)(() =>
                        {
                            DynJsonApi api = new DynJsonApi(S4JExecutor.Current);
                            return api;
                        }));
                        JsApiDefault.Get().AddDelegate("db", (Func<Object, DbApi>)((Parameter) =>
                        {
                            string sourceName = UniConvert.ToString(Parameter);
                            string connectionString = !string.IsNullOrEmpty(sourceName) ?
                                    S4JExecutor.Current.Sources.Get(sourceName) :
                                    S4JExecutor.Current.Sources.GetDefault();

                            DbApi api = new DbApi(connectionString, sourceName ?? S4JExecutor.Current.Sources.DefaultSourceName);
                            return api;
                        }));
                        JsApiDefault.Get().AddDelegate("sqlbuilder", (Func<SqlBuilder>)(() =>
                        {
                            SqlBuilder sql = new SqlBuilder();
                            return sql;
                        }));
                    }

            return i;
        }
    }

    public class JsApi
    {
        private Dictionary<String, Object> cache;

        public IEnumerable<KeyValuePair<String, Object>> Items
        {
            get { return cache; }
        }

        public S4JExecutor Executor { get; set; }

        public JsApi()
        {
            cache = new Dictionary<string, object>();
        }

        public void AddType(String Name, Type TypeName)
        {
            cache[Name] = TypeName;
        }

        public void AddDelegate(String Name, Delegate Delegate)
        {
            cache[Name] = Delegate;
        }

        public void AddValue(String Name, Object Object)
        {
            cache[Name] = Object;
        }
    }


    public class JsEvaluator : IEvaluator
    {
        public async Task<Object> Evaluate(S4JExecutor Executor, S4JToken token, IDictionary<String, object> variables)
        {
            S4JTokenFunction function = token as S4JTokenFunction;
            StringBuilder code = new StringBuilder();

            var engine = new Engine(cfg =>
            {
                cfg.AllowClr();
                cfg.Culture(CultureInfo.InvariantCulture);
            });

            foreach (KeyValuePair<string, object> keyAndVal in JsApiDefault.Get().Items)
            {
                engine.SetValue(keyAndVal.Key, keyAndVal.Value);
            }

            foreach (KeyValuePair<string, object> keyAndVal in variables)
            {
                engine.SetValue(keyAndVal.Key, keyAndVal.Value);
            }

            code.Append(function.ToJsonWithoutGate());

            Stopwatch st = Stopwatch.StartNew();
            try
            {
                engine.Execute(code.ToString());
                object result = engine.GetCompletionValue().ToObject();

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
