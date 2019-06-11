using DynJson.Executor;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JStateFunction : S4JState
    {
        public String FunctionName { get; set; }

        public String SourceName { get; set; }

        ////////////////////////////////////////

        public TagExecutor FunctionTagExecutor { get; set; }

        public S4JState BracketsDefinition { get; set; }

        public S4JState CommentDefinition { get; set; }

        public S4JState QuotationDefinition { get; set; }

        public IEvaluator Evaluator { get; set; }

        public Boolean ReturnExactValue { get; set; }

        ////////////////////////////////////////

        public S4JStateFunction(String FunctionName, String SourceName)
        {
            //if (FunctionNames == null || FunctionNames.Length == 0)
            //    throw new Exception("Function state should have at least one alias!");

            this.FunctionName = FunctionName;
            this.SourceName = SourceName;
            this.Priority = 0;
            this.StateType = EStateType.FUNCTION;
            this.AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_COMMENT,
                    EStateType.FUNCTION_BRACKETS,
                };

            this.Gates.Add(new S4JStateGate()
            {
                Start = (FunctionName + "(").ToCharArray(),
                End = ")".ToCharArray()
            });
        }
    }
}
