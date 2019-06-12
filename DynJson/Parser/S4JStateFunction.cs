using DynJson.Executor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JStateFunction : S4JState
    {
        public List<String> FunctionNames { get; set; }

        // public String SourceName { get; set; }

        ////////////////////////////////////////

        public TagExecutor FunctionTagExecutor { get; set; }

        public S4JState BracketsDefinition { get; set; }

        public S4JState CommentDefinition { get; set; }

        public S4JState QuotationDefinition { get; set; }

        public IEvaluator Evaluator { get; set; }

        ////////////////////////////////////////

        public Boolean ReturnExactValue { get; set; }

        public Boolean ReturnManyObjects { get; set; }

        public Boolean ReturnSingleObject { get; set; }

        public Boolean ReturnSingleValue { get; set; }

        ////////////////////////////////////////

        public S4JStateFunction(String FunctionNames) // , String SourceName)
        {
            //if (FunctionNames == null || FunctionNames.Length == 0)
            //    throw new Exception("Function state should have at least one alias!");

            this.FunctionNames = FunctionNames.Split(';').Select(i => i.Trim().ToLower()).Where(i => i != "").OrderByDescending(i => i).ToList();
            // this.SourceName = SourceName;
            this.Priority = 0;
            this.StateType = EStateType.FUNCTION;
            this.AllowedStateTypes = new[]
                {
                    EStateType.FUNCTION_COMMENT,
                    EStateType.FUNCTION_BRACKETS,
                };

            foreach (var functionName in this.FunctionNames.OrderByDescending(n => n.Length))
            {
                this.Gates.Add(new S4JStateGate()
                {
                    Start = (functionName + "(").ToCharArray(),
                    End = ")".ToCharArray()
                });
            }
        }
    }
}
