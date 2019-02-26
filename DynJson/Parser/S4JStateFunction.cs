using DynJson.Executor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JStateFunction : S4JState
    {
        public String FunctionName { get; set; }

        // public String Source { get; set; }

        ////////////////////////////////////////

        public TagExecutor FunctionTagExecutor { get; set; }

        public S4JState BracketsDefinition { get; set; }

        public S4JState CommentDefinition { get; set; }

        public S4JState QuotationDefinition { get; set; }

        public IEvaluator Evaluator { get; set; }

        ////////////////////////////////////////

        public S4JStateFunction(String FunctionName/*, String Source*/)
        {
            this.FunctionName = FunctionName;
            // this.Source = Source;
            this.IsValue = true;
            this.IsFunction = true;
            this.Priority = 0;
            this.StateType = EStateType.FUNCTION;
            this.AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_COMMENT,
                    EStateType.FUNCTION_BRACKETS,
                };
            this.Gates = new List<S4JStateGate>()
                {
                    new S4JStateGate()
                    {
                        Start = (FunctionName + "(").ToCharArray(),
                        End = ")".ToCharArray()
                    }
                };
        }
    }
}
