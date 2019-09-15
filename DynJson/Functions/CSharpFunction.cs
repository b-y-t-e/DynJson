using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
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
using DynJson.Database;
using System.Diagnostics;

namespace DynJson.Functions
{
    public class CSharpFunction : S4JStateFunction
    {
        public CSharpFunction() :
            this("cs")
        {

        }

        public CSharpFunction(string aliasName) :
            base(aliasName)
        {
            Priority = 0;
            BracketsDefinition = new CSharpBrackets();
            CommentDefinition = new CSharpComment();
            QuotationDefinition = new CSharpQuotation();
            Evaluator = new CSharpEvaluator();
        }
    }

    public class CSharpComment : S4JState
    {
        public CSharpComment()
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
                        Start = "//".ToCharArray(),
                        End = "\n".ToCharArray()
                    }
                };
        }
    }

    public class CSharpQuotation : S4JState
    {
        public CSharpQuotation()
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

    public class CSharpBrackets : S4JState
    {
        public CSharpBrackets()
        {
            Priority = 3;
            StateType = EStateType.FUNCTION_BRACKETS;
            AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_BRACKETS,
                    EStateType.FUNCTION_COMMENT
                };
            // IsValue = true;
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

    public class CSharpEvaluatorGlobals
    {
        public dynamic Globals { get; set; }

        public CSharpEvaluatorGlobals()
        {
            Globals = new ExpandoObject();
        }
    }

    public class CSharpEvaluator : IEvaluator
    {
        public async Task<Object> Evaluate(S4JExecutor Executor, S4JToken token, IDictionary<String, object> variables)
        {
            S4JTokenFunction function = token as S4JTokenFunction;
            StringBuilder code = new StringBuilder();

            CSharpEvaluatorGlobals globals = new CSharpEvaluatorGlobals();
            IDictionary<string, object> globalVariables = globals.Globals as IDictionary<string, object>;

            foreach (KeyValuePair<string, object> keyAndVal in variables)
                globalVariables[keyAndVal.Key] = keyAndVal.Value;

            if (!globalVariables.ContainsKey("db"))
            {
                globalVariables["_dynjsonexecutor"] = Executor;
                code.Append(@"
                    DynJson.Database.DbApi db(object targetSource = null) 
                    { 
                        string sourceName = DynJson.Helpers.CoreHelpers.UniConvert.ToString(targetSource);
                        string connectionString = !string.IsNullOrEmpty(sourceName) ?
                            Globals._dynjsonexecutor.Sources.Get(sourceName) :
                            Globals._dynjsonexecutor.Sources.GetDefault();

                        return new DynJson.Database.DbApi(connectionString, sourceName ?? Globals._dynjsonexecutor.Sources.DefaultSourceName);
                    }
                ");
            }

            if (!globalVariables.ContainsKey("api"))
            {
                Func<DynJsonApi> getApiDelegate = () => new DynJsonApi(Executor);
                globalVariables["api"] = getApiDelegate;
                code.Append(@"
                    var api = Globals.api;
                ");
            }

            foreach (KeyValuePair<string, object> keyAndVal in variables)
                code.Append("var ").Append(keyAndVal.Key).Append(" = ").Append("Globals.").Append(keyAndVal.Key).Append(";");

            code.Append(function.ToJsonWithoutGate());

            var refs = new List<MetadataReference>{
                MetadataReference.CreateFromFile(typeof(DynJson.Database.DbApi).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};

            var imports = ScriptOptions.Default.
                WithImports(
                    "System",
                    "System.Text",
                    "System.Linq",
                    "System.Collections",
                    "System.Collections.Generic").
                WithReferences(refs);

            Stopwatch st = Stopwatch.StartNew();
            try
            {
                object result = await CSharpScript.EvaluateAsync(
                    code.ToString(),
                    imports,
                    globals);

                return result;
            }
            catch (Exception ex)
            {
                if (Logger.IsEnabled)
                    Logger.LogError("CSHARP", "eval", ex.Message, code.ToString());
                throw;
            }
            finally
            {
                if (Logger.IsEnabled)
                    Logger.LogPerformance("CSHARP", "eval", st.ElapsedMilliseconds, code.ToString());
            }
        }
    }
}
