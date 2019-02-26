using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenFactory
    {
        public S4JToken To_token(S4JState State)
        {
            S4JToken result = null;

            if (State != null)
            {
                if (State.StateType == EStateType.S4J)
                    result = new S4JTokenRoot();

                if (State.StateType == EStateType.S4J_ARRAY)
                    result = new S4JTokenArray();

                if (State.StateType == EStateType.S4J_COMMENT)
                    result = new S4JTokenComment();

                if (State.StateType == EStateType.S4J_OBJECT)
                    result = new S4JTokenObject();

                if (State.StateType == EStateType.S4J_TAG)
                    result = new S4JTokenTag();

                if (State.StateType == EStateType.S4J_PARAMETERS)
                    result = new S4JTokenParameters();

                if (State.StateType == EStateType.S4J_QUOTATION)
                    result = new S4JTokenQuotation();
                
                if (State.StateType == EStateType.S4J_TEXT_VALUE)
                    result = new S4JTokenTextValue();

                //if (State.StateType == EStateType.S4J_SEPARATOR)
                //    result = new JsArray();

                //if (State.StateType == EStateType.S4J_SIMPLE_VALUE)
                //    result = new S4JTokenSimpleValue();

                if (State.StateType == EStateType.S4J_OBJECT_CONTENT)
                    result = new S4JTokenObjectContent();

                //if (State.StateType == EStateType.S4J_VALUE_DELIMITER)
                //    result = new JsArray();

                if (State.StateType == EStateType.FUNCTION)
                    result = new S4JTokenFunction()
                    {
                        Evaluator = (State as S4JStateFunction)?.Evaluator
                    };

                if (State.StateType == EStateType.FUNCTION_COMMENT)
                    result = new S4JTokenFunctionComment();

                if (State.StateType == EStateType.FUNCTION_BRACKETS)
                    result = new S4JTokenFunctionBracket();

                if (State.StateType == EStateType.FUNCTION_QUOTATION)
                    result = new S4JTokenFunctionQuotation();
            }

            if (result != null)
                result.State = State;

            return result;
        }
    }
}
