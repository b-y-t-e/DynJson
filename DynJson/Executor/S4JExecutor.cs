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

        public List<TagValidator> TagValidators { get; set; }

        public Methods Methods { get; set; }

        private Dictionary<string, Object> globalVariables;

        public S4JExecutor(S4JStateBag StateBag)
        {
            this.globalVariables = new Dictionary<string, object>();
            this.StateBag = StateBag;
            this.Sources = new Sources();
            this.TagValidators = new List<TagValidator>();
            this.Methods = new Methods(this);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            S4JToken MethodDefinition)
        {
            return await ExecuteWithParameters(
                MethodDefinition,
                (IList)null);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            String MethodDefinitionAsString)
        {
            return await ExecuteWithParameters(
                MethodDefinitionAsString,
                (IList)null);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            S4JToken MethodDefinition,
            IEnumerable<string> ParametersAsJson)
        {
            IList parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => DeserializeJsonParameter(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(
                MethodDefinition,
                parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            String MethodDefinitionAsString,
            IEnumerable<string> ParametersAsJson)
        {
            IList parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => DeserializeJsonParameter(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(MethodDefinitionAsString, parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            S4JToken MethodDefinition,
            IEnumerable<S4JExecutorParam> ParametersAsJson)
        {
            IList parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => DeserializeJsonParameter(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(
                MethodDefinition,
                parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            String MethodDefinitionAsString,
            IEnumerable<S4JExecutorParam> ParametersAsJson)
        {
            IList parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    Select(p => DeserializeJsonParameter(p)).
                    ToArray();
            }

            return await ExecuteWithParameters(MethodDefinitionAsString, parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            String MethodDefinitionAsString,
            Dictionary<string, string> ParametersAsJson)
        {
            Dictionary<string, object> parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    ToDictionary(
                        p => p.Key,
                        p => DeserializeJsonParameter(p.Value));
            }

            S4JTokenRoot methodDefinition = Parse(MethodDefinitionAsString);

            return await ExecuteWithParameters(methodDefinition, parameters);
        }

        async public Task<S4JToken> ExecuteWithJsonParameters(
            S4JTokenRoot MethodDefinition,
            Dictionary<string, string> ParametersAsJson)
        {
            Dictionary<string, object> parameters = null;

            if (ParametersAsJson != null)
            {
                parameters = ParametersAsJson.
                    ToDictionary(
                        p => p.Key,
                        p => DeserializeJsonParameter(p.Value));
            }

            return await ExecuteWithParameters(MethodDefinition, parameters);
        }

        async public Task<S4JToken> ExecuteWithParameters(
            String MethodDefinitionAsString,
            IList Parameters)
        {
            S4JTokenRoot methodDefinition = Parse(MethodDefinitionAsString);

            return await ExecuteWithParameters(methodDefinition, Parameters);
        }

        async public Task<S4JToken> ExecuteWithParameters(
            String MethodDefinitionAsString)
        {
            S4JTokenRoot methodDefinition = Parse(MethodDefinitionAsString);

            return await ExecuteWithParameters(methodDefinition, (IList)null);
        }

        public S4JTokenRoot Parse(
            String MethodDefiniton)
        {
            S4JTokenRoot methodDefinition = new S4JParser().
                Parse(MethodDefiniton, StateBag);

            return methodDefinition;
        }

        async public Task<S4JToken> ExecuteWithParameters(
            S4JToken MethodDefinition,
            IList Parameters)
        {
            Dictionary<string, object> parametersAsDict = new Dictionary<string, object>();
            if (MethodDefinition is S4JTokenRoot root)
            {
                if (Parameters != null)
                {
                    string[] rootParameters = root.ParametersValues.Keys.ToArray();
                    var rootIndex = 0;
                    for (var i = 0; i < Parameters.Count; i++)
                    {
                        object parameterValue = Parameters[i];
                        if (parameterValue is S4JExecutorParam dynParam)
                        {
                            parametersAsDict[dynParam.Name] = DeserializeJsonParameter(dynParam.Value);
                        }
                        else
                        {
                            if (rootIndex < rootParameters.Length)
                            {
                                string parameterName = rootParameters[rootIndex];
                                parametersAsDict[parameterName] = parameterValue;
                                rootIndex++;
                            }
                        }
                    }
                }
            }

            return await ExecuteWithParameters(
                MethodDefinition,
                parametersAsDict);
        }

        async public Task<S4JToken> ExecuteWithParameters(
            S4JToken MethodDefinition,
            Dictionary<string, object> Parameters)
        {
            if (MethodDefinition is S4JTokenRoot root)
            {
                if (Parameters != null)
                    foreach (string parameterName in Parameters.Keys)
                        root.ParametersValues[parameterName] = Parameters[parameterName];

                // validate parameters
                foreach (var rootParameter in root.ParametersValues.ToArray())
                {
                    string rootParameterName = rootParameter.Key;

                    S4JFieldDescription fieldDescription = null;
                    root.ParametersDefinitions.TryGetValue(rootParameterName, out fieldDescription);

                    if (fieldDescription == null)
                        continue;

                    if (fieldDescription.Type == S4JFieldType.TOKEN)
                    {
                        foreach (var parameter in Parameters)
                            globalVariables[parameter.Key] = parameter.Value;

                        await Evaluate(fieldDescription.Token);
                        root.ParametersValues[rootParameterName] = fieldDescription.Token.Result;

                        globalVariables.Clear();
                    }
                    else
                    {
                        ValidateFieldDescription(fieldDescription, rootParameter.Value);
                    }
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

            IDictionary<string, object> variables = GetExecutingVariables(null, token);

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
            else if (token is S4JTokenTextValue textValue && textValue.VariableNameFromText != null)
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
                    if (token.WasRemoved)
                        break;
                }

                if (!token.WasRemoved)
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
                GetValueFromPath(variables, token.VariableNameFromText);

            // object value = null;
            //variables.TryGetValue(token.VariablePath, out value);
            token.Result = value;
        }

        async private Task EvaluateFunction(S4JTokenFunction function)
        {
            if (function == null)
                return;

            S4JStateFunction stateFunction = function.State as S4JStateFunction;
            IDictionary<String, object> variables = GetExecutingVariables(function, function?.Parent);

            if (stateFunction.FunctionTagExecutor != null)
            {
                using (ExecutorContext context = new ExecutorContext(function, variables))
                    stateFunction.FunctionTagExecutor(context);
            }

            object result = await function.
                Evaluator?.
                Evaluate(this, function, variables);

            if (stateFunction.ResultType == S4JFunctionResult.SINGE_RESULT)
            {
                result = S4jExecutorResultExtractor.GetSingleObjectFromResult(result);
            }
            else if (stateFunction.ResultType == S4JFunctionResult.SINGLE_SCALAR)
            {
                result = S4jExecutorResultExtractor.GetSingleAndFirstValueFromResult(result);
            }
            else if (stateFunction.ResultType == S4JFunctionResult.MANY_RESULTS)
            {
                result = S4jExecutorResultExtractor.GetManyObjectsFromResult(result);
            }

            function.IsEvaluated = true;
            function.Result = result;

            if (function.IsObjectSingleKey || function.InArray)
            {
                // { a : 1, q(select id, nazwa from towar where id = 123) } -> { a : 1, id : 123, nazwa : 'nazwa' }
                if (function.Parent is S4JTokenObject objectParent3 &&
                    function.IsObjectSingleKey)
                {
                    await EvaluateFunctionInsideObject(
                        objectParent3,
                        function,
                        result);
                }
                // [ @(1), @(null) ] -> [ 1, null ]
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
            else
            {
                function.JsonFromResult = true;
            }
        }

        private async Task EvaluateFunctionInsideObject(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            if (function == null)
                return;

            if (objectParent.Parent is S4JTokenArray)
            {
                await EvaluateFunctionInsideObjectInsideArray(
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

        private async Task EvaluateFunctionInsideObjectInsideArray(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            IList<Object> list = S4jExecutorResultExtractor.GetManyObjectsFromResult(result);

            if (objectParent.Children.Count == 1)
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = S4jTokenConverter.ConvertToToken(list, function.IsVisible).ToArray();

                objectParent.Parent.RemoveChild(
                    objectParent,
                    tokensFromResult);
            }
            else
            {
                Int32 indexOfFun = objectParent.IndexOfChild(function);

                IList<S4JToken> tokensFromResult = S4jTokenConverter.ConvertToManyTokens(list, function.IsVisible).ToArray();

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

                objectParent.WasRemoved = true;

                objectParent.Parent.RemoveChild(
                    objectParent,
                    newTokens);

                foreach (var newToken in newTokens)
                {
                    for (var i = 0; i < newToken.Children.Count; i++)
                    {
                        if (i == indexOfFun)
                            continue;

                        S4JToken child = newToken.Children[i];
                        await Evaluate(child);
                    }

                    AfterEvaluateToken(newToken);
                }
            }

            // function.Result = list;
        }

        private void EvaluateFunctionInsideObjectInsideAnyOther(
            S4JTokenObject objectParent,
            S4JTokenFunction function,
            object result)
        {
            var item = S4jExecutorResultExtractor.GetSingleObjectFromResult(result);

            IList<S4JToken> tokens = S4jTokenConverter.ConvertToTokens(item, function.IsVisible, true).ToArray();

            if (function.IsObjectSingleKey &&
                objectParent.Children.Count == 1 &&
                tokens.Count == 0)
            {
                objectParent.Parent.RemoveChild(
                    objectParent,
                    S4jTokenConverter.GetNullToken(function.IsVisible));
            }
            else
            {
                objectParent.RemoveChild(
                    function,
                    tokens);
            }
        }

        /*private void TurnParentIntoNull(
            S4JTokenObject objectParent)
        {
            S4JToken parentOfParent = objectParent.Parent;
            parentOfParent.RemoveChild(objectParent, ConvertToTokens((object)null, true, false).ToArray());
        }

        private void RemoveChildInParent(
            S4JTokenObject objectParent,
            S4JTokenFunction function)
        {
            objectParent.RemoveChild(function);
        }*/

        private void EvaluateFunctionInsideArray(
            S4JTokenArray arrayParent,
            S4JTokenFunction function,
            object result)
        {
            var list = S4jExecutorResultExtractor.GetListOfSingleObjectsFromResult(result);

            IList<S4JToken> tokens = S4jTokenConverter.ConvertToToken(list, function.IsVisible).ToArray();

            function.Parent.RemoveChild(
                function,
                tokens);
        }

        private void EvaluateFunctionInsideAnyOther(
            S4JTokenFunction function,
            object result)
        {
            var item = S4jExecutorResultExtractor.GetSingleAndFirstValueFromResult(result);

            IList<S4JToken> tokens = S4jTokenConverter.ConvertToTokens(item, function.IsVisible, false).ToArray();

            String text = JsonSerializer.SerializeJson(result);
            function.Children.Clear();
            function.Children.AddRange(tokens);

            // function.Result = item;
        }

        private IDictionary<String, object> GetExecutingVariables(S4JToken thisToken, S4JToken parentToken)
        {
            Dictionary<String, object> variables = new Dictionary<string, object>();

            string objectKeyName = null;
            if (thisToken != null && thisToken.IsObjectValue && thisToken.PrevToken != null)
            {
                objectKeyName = UniConvert.ToString(thisToken.PrevToken.Result);
            }

            {
                S4JToken currentToken = parentToken;
                while (currentToken != null)
                {
                    Dictionary<string, object> parentParameters = currentToken.GetParameters();
                    if (parentParameters != null)
                    {
                        foreach (KeyValuePair<string, object> keyAndVal in parentParameters)
                        {
                            if (currentToken == parentToken && objectKeyName == keyAndVal.Key)
                                continue;

                            if (!variables.ContainsKey(keyAndVal.Key))
                                variables[keyAndVal.Key] = keyAndVal.Value;
                        }
                    }
                    currentToken = currentToken.Parent;
                }
            }

            foreach (var variable in globalVariables)
                variables[variable.Key] = variable.Value;

            return variables;
        }


        Object ValidateFieldDescription(S4JFieldDescription FieldDescription, Object Value)
        {
            if (FieldDescription.Type == S4JFieldType.ANY)
                return Value;

            if (FieldDescription.IsRequired && Value == null)
                throw new S4JNullParameterException("Parameter " + FieldDescription.Name + " cannot be null");

            if (Value != null && FieldDescription.Type == S4JFieldType.BOOL)
                if (!MyTypeHelper.IsBoolean(Value.GetType()))
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type boolean");

            if (Value != null && FieldDescription.Type == S4JFieldType.DATETIME)
                if (!MyTypeHelper.IsDateTime(Value.GetType()))
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type datetime");

            if (Value != null && FieldDescription.Type == S4JFieldType.FLOAT)
                if (!MyTypeHelper.IsNumeric(Value.GetType()))
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type float");

            if (Value != null && FieldDescription.Type == S4JFieldType.INT)
                if (!MyTypeHelper.IsInteger(Value.GetType()))
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type integer");

            if (Value != null && FieldDescription.Type == S4JFieldType.STRING)
                if (!MyTypeHelper.IsString(Value.GetType()))
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type string");

            if (FieldDescription.Type == S4JFieldType.ARRAY)
                if (Value == null)
                {
                    return new List<Object>();
                }
                else if (!(Value is IList))
                {
                    //if (MyTypeHelper.IsClass(Value.GetType()) || Value is IDictionary<String, Object>)
                    //    return new List<Object>() { Value };
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type array");
                }

            if (Value != null && FieldDescription.Type == S4JFieldType.OBJECT)
                if (!(MyTypeHelper.IsClass(Value.GetType()) || Value is IDictionary<String, Object>) || Value is IList)
                    throw new S4JInvalidParameterTypeException("Parameter " + FieldDescription.Name + " should be of type object");

            return Value;
        }

        object DeserializeJsonParameter(Object value)
        {
            if (value == null)
                return null;

            if (value is S4JExecutorParam)
            {
                return value;
            }
            else
            {
                return JsonToDynamicDeserializer.Deserialize((string)value);
            }
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

    public class S4JExecutorParam
    {
        public String Name;
        public Object Value;
        public S4JExecutorParam(String Name, Object Value)
        {
            this.Name = Name;
            this.Value = Value;
        }
    }

    public static class S4jExecutorResultExtractor
    {
        public static IList<Object> GetManyObjectsFromResult(Object value, Boolean AnalyseSubValues = true)
        {
            if (value == null)
                return null;

            List<Object> list = new List<object>();

            if (MyTypeHelper.IsPrimitive(value.GetType()))
                list.Add(value);

            else if (value is IDictionary<String, Object>)
                list.Add(value);

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

        public static Object GetSingleObjectFromResult(Object value)
        {
            if (value == null)
                return null;

            if (MyTypeHelper.IsPrimitive(value.GetType()))
            {
                return value;
                /*return new Dictionary<string, object>
                {
                    { "value", value }
                };*/
            }

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

        public static List<Object> GetListOfSingleObjectsFromResult(Object value, Boolean AnalyseSubValues = true)
        {
            if (value == null)
                return null;

            List<Object> list = new List<object>();

            if (MyTypeHelper.IsPrimitive(value.GetType()))
            {
                list.Add(value);
            }

            else if (value is IDictionary<String, Object> dict)
            {
                list.Add(value);

                /*if (dict.Count > 0)
                    list.Add(dict.First().Value);*/
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
                list.Add(ReflectionHelper.ToDictionary(value));
                /*return (IDictionary<String, Object>)value;
                return ReflectionHelper.ToDictionary(value);

                Dictionary<string, object> dictForValue = ReflectionHelper.ToDictionary(value);
                if (dictForValue.Count > 0)
                    list.Add(dictForValue.First().Value);*/
            }

            return list;
        }

        public static Object GetSingleAndFirstValueFromResult(Object value)
        {
            if (value == null)
                return null;

            if (MyTypeHelper.IsPrimitive(value.GetType()))
            {
                return value;
            }

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
                Dictionary<string, object> dictForValue = ReflectionHelper.ToDictionary(value);
                return dictForValue.Count > 0 ? dictForValue.FirstOrDefault().Value : null;
            }

            return null;
        }

    }

    public static class S4jTokenConverter
    {
        public static IEnumerable<S4JToken> ConvertToToken(IList<Object> List, Boolean IsVisible)
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

        public static IEnumerable<S4JToken> ConvertToManyTokens(IList<Object> List, Boolean IsVisible)
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

        public static IList<S4JToken> GetNullToken(Boolean IsVisible)
        {
            return new[] { new S4JTokenObjectContent()
            {
                IsVisible = IsVisible,
                Result = null,
                Text = "null",
                //IsKey = true,
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT }
            }};
        }

        /*public static IEnumerable<S4JToken> ConvertToTokens(IDictionary<String, Object> Dictionary, Boolean IsVisible)
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
        }*/

        public static IEnumerable<S4JToken> ConvertToTokens(Object Value, Boolean IsVisible, Boolean MergeWithObject)
        {
            if (Value == null)
                yield break;

            yield return new S4JTokenObjectContent()
            {
                IsVisible = IsVisible,
                Result = Value,
                Text = MergeWithObject ? Value.SerializeJsonNoBrackets() : Value.SerializeJson(),
                IsObjectSingleKey = true,
                IsCommited = true,
                State = new S4JState() { StateType = EStateType.S4J_OBJECT_CONTENT }
                //State = S4JDefaultStateBag.Get().ValueState // new S4JState() { StateType = EStateType.S4J_TEXT_VALUE, IsValue = true, IsSimpleValue = true }
            };
        }
    }
}
