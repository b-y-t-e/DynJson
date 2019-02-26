using Microsoft.CodeAnalysis.CSharp.Scripting;
using DynJson.Helpers;
using DynJson.Parser;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

using DynJson.Helpers.CoreHelpers;
using DynJson.Tokens;
using DynJson.Classes;

namespace DynJson.Executor
{
    public delegate bool TagValidator(ExecutorContext Context);

    public delegate void TagExecutor(ExecutorContext Context);

    public class ExecutorContext : IDisposable
    {
        public S4JToken Token;
        public IDictionary<String, object> Variables;
        public IDictionary<string, object> Tags { get { return Token?.Tags; } }

        public ExecutorContext(
            S4JToken Token,
            IDictionary<String, object> Variables)
        {
            this.Token = Token;
            this.Variables = Variables;
        }

        public void Dispose()
        {
            Token = null;
            Variables = null;
        }
    }

    public class S4JExecutor
    {
        public S4JStateBag StateBag { get; private set; }

        public Sources Sources { get; private set; }

        public Object Result { get; private set; }

        public TagValidator TagValidator { get; set; }

        public S4JExecutor(S4JStateBag StateBag)
        {
            this.StateBag = StateBag;
            this.Sources = new Sources();
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(String MethodDefinitionAsJson, Dictionary<string, string> ParametersAsJson)
        {
            Dictionary<string, object> parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    ToDictionary(
                        p => p.Key,
                        p => JsonToDynamicDeserializer.Deserialize(p.Value));
            }

            return await ExecuteWithParameters(MethodDefinitionAsJson, parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(String MethodDefinitionAsJson, params String[] ParametersAsJson)
        {
            Object[] parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => JsonToDynamicDeserializer.Deserialize(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(MethodDefinitionAsJson, parameters);
        }

        async public Task<S4JToken> ExecuteWithParameters(String MethodDefinitionAsJson, params Object[] Parameters)
        {
            S4JTokenRoot methodDefinition = new S4JParser().
                Parse(MethodDefinitionAsJson, StateBag);

            return await ExecuteWithParameters(methodDefinition, Parameters);
        }

        async public Task<S4JToken> ExecuteWithParameters(S4JToken MethodDefinition, params Object[] Parameters)
        {
            Dictionary<string, object> parametersAsDict = new Dictionary<string, object>();
            if (MethodDefinition is S4JTokenRoot root)
            {
                if (Parameters != null)
                {
                    int index = 0;
                    foreach (string parameterName in root.Parameters.Keys.ToArray())
                    {
                        object parameterValue = null;
                        if (index < Parameters.Length)
                            parameterValue = Parameters[index];

                        if (index < Parameters.Length)
                            parametersAsDict[parameterName] = parameterValue;

                        index++;
                    }
                }
            }

            return await ExecuteWithParameters(
                MethodDefinition,
                parametersAsDict);
        }

        async public Task<S4JToken> ExecuteWithParameters(S4JToken MethodDefinition, Dictionary<string, object> Parameters)
        {
            if (MethodDefinition is S4JTokenRoot root)
            {
                if (Parameters != null)
                    foreach (string parameterName in Parameters.Keys)
                        root.Parameters[parameterName] = Parameters[parameterName];

                // validate parameters
                foreach (var parameter in root.Parameters)
                {
                    S4JFieldDescription fieldDescription = null;
                    root.ParametersDefinitions.TryGetValue(parameter.Key, out fieldDescription);

                    if (fieldDescription != null)
                        fieldDescription.Validate(parameter.Value);
                }
            }

            await Evaluate(MethodDefinition);

            if (MethodDefinition is S4JTokenRoot)
                return MethodDefinition.Children.LastOrDefault();

            return MethodDefinition;
        }

        async private Task Evaluate(S4JToken token)
        {
            if (token == null)
                return;

            bool canBeEvaluated = true;
            if (token.Tags.Count > 0 && TagValidator != null)
            {
                using (ExecutorContext context = new ExecutorContext(token, GetExecutingVariables(token)))
                    canBeEvaluated = TagValidator(context);
            }

            token.IsVisible = canBeEvaluated;
            if (!canBeEvaluated)
                return;

            if (token is S4JTokenFunction function) //   .State.StateType == EStateType.FUNCTION)
            {
                await EvaluateFunction(function);
            }
            if (token is S4JTokenTextValue textValue && textValue.VariableName != null)
            {
                await EvaluateTokenVariable(textValue);
            }
            else
            {
                var children = token.Children.ToArray();
                for (var i = 0; i < children.Length; i++)
                {
                    S4JToken child = children[i];
                    await Evaluate(child);
                }
            }
        }

        async private Task EvaluateTokenVariable(S4JTokenTextValue token)
        {
            var variables = GetExecutingVariables(token);
            object value = null;
            variables.TryGetValue(token.VariableName, out value);
            token.Value = value;
        }

        async private Task EvaluateFunction(S4JTokenFunction function)
        {
            if (function == null)
                return;

            IDictionary<String, object> variables = GetExecutingVariables(function);

            object result = null;

            bool canBeEvaluated = true;
            if (function.Tags.Count > 0 && TagValidator != null)
            {
                using (ExecutorContext context = new ExecutorContext(function, variables))
                    canBeEvaluated = TagValidator(context);
            }


            function.IsVisible = canBeEvaluated;
            if (canBeEvaluated)
            {
                if (function.State is S4JStateFunction stateFunction &&
                    stateFunction.FunctionTagExecutor != null)
                {
                    using (ExecutorContext context = new ExecutorContext(function, variables))
                        stateFunction.FunctionTagExecutor(context);
                }

                result = await function.Evaluator?.Evaluate(this, function, variables);
            }

            function.IsEvaluated = true;
            function.Result = result;

            if (function.Parent is S4JTokenObject objectParent &&
                function.IsObjectSingleKey)
            {
                EvaluateFunctionInsideObject(
                    objectParent,
                    function,
                    result);
            }
            else if (function.Parent is S4JTokenArray arrayParent)
            {
                EvaluateFunctionInsideArray(
                    arrayParent,
                    function,
                    result);
            }
            else
            {
                EvaluateFunctionInsideAnyOther(
                    function,
                    result);
            }
        }

        private void EvaluateFunctionInsideObject(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            if (function == null)
                return;

            if (objectParent.Parent is S4JTokenArray)
            {
                EvaluateFunctionInsideObjectInsideArray(
                    objectParent,
                    function,
                    result);
            }
            else
            {
                EvaluateFunctionInsideObjectInsideAnyOther(
                    objectParent,
                    function,
                    result);
            }
        }

        private void EvaluateFunctionInsideObjectInsideArray(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            if (objectParent.Children.Count == 1)
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = ConvertToToken(
                    GetManyObjectsFromResult(result)).ToArray();

                objectParent.Parent.RemoveChild(
                    objectParent,
                    tokensFromResult);
            }
            else
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = ConvertToManyTokens(
                    GetManyObjectsFromResult(result)).ToArray();

                List<S4JToken> newTokens = new List<S4JToken>();
                foreach (S4JToken tokenFromResult in tokensFromResult)
                {
                    S4JToken newObjectToken = objectParent.Clone();
                    newObjectToken.Parent = objectParent.Parent;

                    newObjectToken.RemoveChild(
                        indexOfFun,
                        new[] { tokenFromResult });

                    newTokens.Add(newObjectToken);
                }

                objectParent.Parent.RemoveChild(
                    objectParent,
                    newTokens);
            }
        }

        private void EvaluateFunctionInsideObjectInsideAnyOther(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            IList<S4JToken> tokens = ConvertToTokens(
                GetSingleObjectFromResult(result)).ToArray();

            objectParent.RemoveChild(
                function,
                tokens);
        }

        private void EvaluateFunctionInsideArray(
            S4JTokenArray arrayParent,
            S4JTokenFunction function,
            object result)
        {
            IList<S4JToken> tokens = ConvertToToken(
                GetListOfSingleObjectsFromResult(result)).ToArray();

            function.Parent.RemoveChild(
                function,
                tokens);
        }

        private void EvaluateFunctionInsideAnyOther(
            S4JTokenFunction function,
            object result)
        {
            IList<S4JToken> tokens = ConvertToTokens(
                GetSingleAndFirstValueFromResult(result)).ToArray();

            String text = JsonSerializer.SerializeJson(result);
            function.Children.Clear();
            function.Children.AddRange(tokens);
        }

        private IDictionary<String, object> GetExecutingVariables(S4JToken token)
        {
            Dictionary<String, object> variables = new Dictionary<string, object>();
            {
                S4JToken parentToken = token;
                while (parentToken != null)
                {
                    Dictionary<string, object> parentParameters = parentToken.GetParameters();
                    if (parentParameters != null)
                    {
                        foreach (KeyValuePair<string, object> keyAndVal in parentParameters)
                        {
                            if (!variables.ContainsKey(keyAndVal.Key))
                            {
                                variables[keyAndVal.Key] = keyAndVal.Value;
                            }
                        }
                    }
                    parentToken = parentToken.Parent;
                }
            }
            return variables;
        }

        private IEnumerable<S4JToken> ConvertToTokens(IDictionary<String, Object> Dictionary)
        {
            if (Dictionary == null)
                yield break;

            yield return new S4JTokenObjectContent()
            {
                Value = Dictionary,
                Text = Dictionary.SerializeJsonNoBrackets(),
                //IsKey = true,
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT, IsValue = true, IsSimpleValue = true }
            };
        }

        private IEnumerable<S4JToken> ConvertToToken(IList<Object> List)
        {
            if (List == null || List.Count == 0)
                yield break;

            yield return new S4JTokenObjectContent()
            {
                Value = List,
                Text = List.SerializeJsonNoBrackets(),
                //IsKey = true,
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT, IsValue = true, IsSimpleValue = true }
            };
        }

        private IEnumerable<S4JToken> ConvertToManyTokens(IList<Object> List)
        {
            if (List == null)
                yield break;

            foreach (Object item in List)
                yield return new S4JTokenObjectContent()
                {
                    Value = item,
                    Text = item.SerializeJsonNoBrackets(),
                    //IsKey = true,
                    IsObjectSingleKey = true,
                    IsCommited = true,
                    State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT, IsValue = true, IsSimpleValue = true }
                };
        }

        private IEnumerable<S4JToken> ConvertToTokens(Object Value)
        {
            //if (Value == null)
            //    yield break;

            yield return new S4JTokenTextValue()
            {
                Text = Value.SerializeJson(),
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_TEXT_VALUE, IsValue = true, IsSimpleValue = true }
            };
        }

        private IList<Object> GetManyObjectsFromResult(Object value, Boolean AnalyseSubValues = true)
        {
            if (value == null)
                return null;

            List<Object> list = new List<object>();

            if (MyTypeHelper.IsPrimitive(value.GetType()))
                list.Add(value);

            else if (value is IDictionary<String, Object>)
            {
                list.Add(value);
            }

            else if (value is ICollection)
            {
                if (AnalyseSubValues)
                {
                    foreach (Object subValue in (ICollection)value)
                        list.AddRange(GetManyObjectsFromResult(subValue, false));
                }
                else
                {
                    list.Add(value);
                }
            }

            else if (value.GetType().IsClass)
            {
                list.Add(value);
            }

            return list;
        }

        private IDictionary<String, Object> GetSingleObjectFromResult(Object value)
        {
            if (value == null)
                return null;

            if (MyTypeHelper.IsPrimitive(value.GetType()))
                return new Dictionary<string, object>
                {
                    { "value", value }
                };

            else if (value is IDictionary<String, Object>)
            {
                return (IDictionary<String, Object>)value;
            }

            else if (value is ICollection)
            {
                foreach (Object subValue in (ICollection)value)
                    return GetSingleObjectFromResult(subValue);
            }

            else if (value.GetType().IsClass)
            {
                return ReflectionHelper.ToDictionary(value);
            }

            return null;
        }

        private List<Object> GetListOfSingleObjectsFromResult(Object value, Boolean AnalyseSubValues = true)
        {
            if (value == null)
                return null;

            List<Object> list = new List<object>();

            if (MyTypeHelper.IsPrimitive(value.GetType()))
                list.Add(value);

            else if (value is IDictionary<String, Object> dict)
            {
                if (dict.Count > 0)
                    list.Add(dict.First().Value);
            }

            else if (value is ICollection)
            {
                if (AnalyseSubValues)
                {
                    foreach (Object subValue in (ICollection)value)
                        list.AddRange(GetListOfSingleObjectsFromResult(subValue, false));
                }
                else
                {
                    list.Add(value);
                }
            }

            else if (value.GetType().IsClass)
            {
                var dictForValue = ReflectionHelper.ToDictionary(value);
                if (dictForValue.Count > 0)
                    list.Add(dictForValue.First().Value);
            }

            return list;
        }

        private Object GetSingleAndFirstValueFromResult(Object value)
        {
            if (value == null)
                return null;

            if (MyTypeHelper.IsPrimitive(value.GetType()))
                return value;

            else if (value is IDictionary<String, Object> dict)
            {
                return dict.Count > 0 ? dict.FirstOrDefault().Value : null;
            }

            else if (value is ICollection)
            {
                foreach (Object subValue in (ICollection)value)
                    return GetSingleAndFirstValueFromResult(subValue);
            }

            else if (value.GetType().IsClass)
            {
                var dictForValue = ReflectionHelper.ToDictionary(value);
                return dictForValue.Count > 0 ? dictForValue.FirstOrDefault().Value : null;
            }

            return null;
        }
    }

    public interface IEvaluator
    {
        Task<Object> Evaluate(S4JExecutor Executor, S4JToken node, IDictionary<String, object> variables);
    }

    public class ExecutionTree
    {
        public S4JToken Root;

        public ExecutionTreeNode RootExecutionNode;

        public void Build(S4JToken Root)
        {

        }
    }

    public class ExecutionTreeNode
    {
        public S4JToken Node;

        public Dictionary<String, Object> Variables;

        public List<S4JToken> Dependencies;

        public ExecutionTreeNode()
        {
            Variables = new Dictionary<string, object>();
            Dependencies = new List<S4JToken>();
        }
    }
}
