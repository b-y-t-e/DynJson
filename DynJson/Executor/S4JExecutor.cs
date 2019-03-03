﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
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

        public List<TagValidator> TagValidators { get; set; }

        private Dictionary<string, Object> globalVariables;

        public S4JExecutor(S4JStateBag StateBag)
        {
            this.globalVariables = new Dictionary<string, object>();
            this.StateBag = StateBag;
            this.Sources = new Sources();
            this.TagValidators = new List<TagValidator>();
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

        async public Task<S4JToken> ExecuteWithJsonParameters(S4JTokenRoot MethodDefinition, Dictionary<string, string> ParametersAsJson)
        {
            Dictionary<string, object> parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    ToDictionary(
                        p => p.Key,
                        p => JsonToDynamicDeserializer.Deserialize(p.Value));
            }

            return await ExecuteWithParameters(MethodDefinition, parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(S4JToken MethodDefinition, params String[] ParametersAsJson)
        {
            object[] parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => JsonToDynamicDeserializer.Deserialize(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(
                MethodDefinition,
                parameters);
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
            S4JTokenRoot methodDefinition = Parse(MethodDefinitionAsJson);

            return await ExecuteWithParameters(methodDefinition, Parameters);
        }

        public S4JTokenRoot Parse(String MethodDefiniton)
        {
            S4JTokenRoot methodDefinition = new S4JParser().
                Parse(MethodDefiniton, StateBag);

            return methodDefinition;
        }

        async public Task<S4JToken> ExecuteWithParameters(S4JToken MethodDefinition, params Object[] Parameters)
        {
            Dictionary<string, object> parametersAsDict = new Dictionary<string, object>();
            if (MethodDefinition is S4JTokenRoot root)
            {
                if (Parameters != null)
                {
                    int index = 0;
                    foreach (string parameterName in root.ParametersValues.Keys.ToArray())
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
                        root.ParametersValues[parameterName] = Parameters[parameterName];

                // validate parameters
                foreach (var parameter in root.ParametersValues)
                {
                    S4JFieldDescription fieldDescription = null;
                    root.ParametersDefinitions.TryGetValue(parameter.Key, out fieldDescription);

                    if (fieldDescription != null)
                        fieldDescription.Validate(parameter.Value);
                }
            }

            await Evaluate(MethodDefinition);

            if (MethodDefinition is S4JTokenRoot)
            {
                S4JToken resultToken = MethodDefinition.
                    GetLastVisibleChild<S4JTokenRootObject>()?.
                    GetLastVisibleChild();

                if (resultToken != null)
                    return resultToken;

                resultToken = MethodDefinition.
                    GetLastVisibleChild();

                return resultToken;
            }

            return MethodDefinition;
        }

        async private Task Evaluate(S4JToken token)
        {
            if (token == null)
                return;

            IDictionary<string, object> variables = GetExecutingVariables(token);

            bool isVisible = true;
            if (token.Tags.Count > 0 && TagValidators?.Count > 0)
            {
                foreach (var tagValidator in TagValidators)
                {
                    using (ExecutorContext context = new ExecutorContext(token, variables))
                    {
                        isVisible = tagValidator(context);
                        if (!isVisible)
                            break;
                    }
                }
            }

            if (token.IsVisible && !isVisible)
                token.IsVisible = false;
            //if (!canBeEvaluated)
            //    return;

            if (token is S4JTokenFunction function) // .State.StateType == EStateType.FUNCTION)
            {
                await EvaluateFunction(function);
                AfterEvaluateToken(function);
            }
            else if (token is S4JTokenTextValue textValue && textValue.VariableName != null)
            {
                await EvaluateTokenVariable(textValue, variables);
                AfterEvaluateToken(textValue);
            }
            else
            {
                var children = token.Children.ToArray();
                for (var i = 0; i < children.Length; i++)
                {
                    S4JToken child = children[i];
                    await Evaluate(child);
                }
                AfterEvaluateToken(token);
            }
        }

        void AfterEvaluateToken(S4JToken token)
        {
            if (token.OutputVariableName == null)
                return;

            object obj = token.Result;
            if (obj == null)
            {
                string json = token.ToJson(true);

                obj = JsonToDynamicDeserializer.
                    Deserialize(json);
            }

            globalVariables[token.OutputVariableName] = obj;
        }

        async private Task EvaluateTokenVariable(S4JTokenTextValue token, IDictionary<string, object> variables)
        {
            Object value = MyReflectionHelper.
                GetValueFromPath(variables, token.VariableName);

            // object value = null;
            //variables.TryGetValue(token.VariablePath, out value);
            token.Result = value;
        }

        async private Task EvaluateFunction(S4JTokenFunction function)
        {
            if (function == null)
                return;

            S4JStateFunction stateFunction = function.State as S4JStateFunction;
            IDictionary<String, object> variables = GetExecutingVariables(function?.Parent);

            if (stateFunction.FunctionTagExecutor != null)
            {
                using (ExecutorContext context = new ExecutorContext(function, variables))
                    stateFunction.FunctionTagExecutor(context);
            }

            object result = await function.Evaluator?.Evaluate(this, function, variables);

            function.IsEvaluated = true;
            function.Result = result;

            if (stateFunction.ReturnExactValue)
            {
                function.JsonFromResult = true;
            }
            else if (function.Parent is S4JTokenObject objectParent &&
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
            var list = GetManyObjectsFromResult(result);

            if (objectParent.Children.Count == 1)
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = ConvertToToken(list, function.IsVisible).ToArray();

                objectParent.Parent.RemoveChild(
                    objectParent,
                    tokensFromResult);
            }
            else
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = ConvertToManyTokens(list, function.IsVisible).ToArray();

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

            // function.Result = list;
        }

        private void EvaluateFunctionInsideObjectInsideAnyOther(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            var item = GetSingleObjectFromResult(result);

            IList<S4JToken> tokens = ConvertToTokens(item, function.IsVisible).ToArray();

            objectParent.RemoveChild(
                function,
                tokens);

            // function.Result = item;
        }

        private void EvaluateFunctionInsideArray(
            S4JTokenArray arrayParent,
            S4JTokenFunction function,
            object result)
        {
            var list = GetListOfSingleObjectsFromResult(result);

            IList<S4JToken> tokens = ConvertToToken(list, function.IsVisible).ToArray();

            function.Parent.RemoveChild(
                function,
                tokens);
        }

        private void EvaluateFunctionInsideAnyOther(
            S4JTokenFunction function,
            object result)
        {
            var item = GetSingleAndFirstValueFromResult(result);

            IList<S4JToken> tokens = ConvertToTokens(item, function.IsVisible).ToArray();

            String text = JsonSerializer.SerializeJson(result);
            function.Children.Clear();
            function.Children.AddRange(tokens);

            // function.Result = item;
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

            foreach (var variable in globalVariables)
                variables[variable.Key] = variable.Value;

            return variables;
        }

        private IEnumerable<S4JToken> ConvertToTokens(IDictionary<String, Object> Dictionary, Boolean IsVisible)
        {
            if (Dictionary == null)
                yield break;

            yield return new S4JTokenObjectContent()
            {
                IsVisible = IsVisible,
                Result = Dictionary,
                Text = Dictionary.SerializeJsonNoBrackets(),
                //IsKey = true,
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT }
            };
        }

        private IEnumerable<S4JToken> ConvertToToken(IList<Object> List, Boolean IsVisible)
        {
            if (List == null || List.Count == 0)
                yield break;

            yield return new S4JTokenObjectContent()
            {
                IsVisible = IsVisible,
                Result = List,
                Text = List.SerializeJsonNoBrackets(),
                //IsKey = true,
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT }
            };
        }

        private IEnumerable<S4JToken> ConvertToManyTokens(IList<Object> List, Boolean IsVisible)
        {
            if (List == null)
                yield break;

            foreach (Object item in List)
                yield return new S4JTokenObjectContent()
                {
                    Result = item,
                    IsVisible = IsVisible,
                    Text = item.SerializeJsonNoBrackets(),
                    //IsKey = true,
                    IsObjectSingleKey = true,
                    IsCommited = true,
                    State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT }
                };
        }

        private IEnumerable<S4JToken> ConvertToTokens(Object Value, Boolean IsVisible)
        {
            //if (Value == null)
            //    yield break;

            yield return new S4JTokenTextValue()
            {
                IsVisible = IsVisible,
                Text = Value.SerializeJson(),
                IsObjectSingleKey = true,
                IsCommited = true,
                State = S4JDefaultStateBag.Get().ValueState // new S4JState() { StateType = EStateType.S4J_TEXT_VALUE, IsValue = true, IsSimpleValue = true }
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
