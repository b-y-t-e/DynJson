using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;

namespace DynJson.Tokens
{
    public class S4JTokenRoot : S4JToken
    {
        public String Name { get; set; }

        public Dictionary<String, S4JFieldDescription> ParametersDefinitions { get; set; }

        public Dictionary<String, Object> ParametersValues { get; set; }

        public Boolean IsFullSpecificationProvided { get; set; }

        //////////////////////////////////////////////////

        public S4JTokenRoot()
        {
            Children = new List<S4JToken>();
            ParametersValues = new Dictionary<string, object>();
            ParametersDefinitions = new Dictionary<string, S4JFieldDescription>();
        }

        public override S4JToken Clone()
        {
            S4JTokenRoot token = base.Clone() as S4JTokenRoot;
            token.ParametersDefinitions = new Dictionary<string, S4JFieldDescription>(this.ParametersDefinitions);
            token.ParametersValues = new Dictionary<string, object>(this.ParametersValues);
            return token;
        }

        public override void AddChildToToken(S4JToken Child)
        {
            base.AddChildToToken(Child);
        }

        public override Dictionary<String, Object> GetParameters()
        {
            return ParametersValues;
        }

        public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            if (!string.IsNullOrEmpty(Name))
            {
                Builder.Append(Name);

                Builder.Append("(");
                Int32 index = 0;
                if (ParametersValues != null)
                    foreach (var parameterDefinition in ParametersDefinitions)
                    {
                        if (index > 0) Builder.Append(",");
                        if (parameterDefinition.Value == null)
                        {
                            Builder.Append($"{parameterDefinition.Key}");
                        }
                        else
                        {
                            Builder.Append($"{parameterDefinition.Key}:{parameterDefinition.Value.ToJson()}");
                        }
                        index++;
                    }
                Builder.Append(")");
            }

            base.BuildJson(Builder, Force);

            return true;
        }

        public override bool Commit()
        {
            base.Commit();

            S4JTokenRoot root = this;

            IList<S4JToken> visibleChildren = root.GetVisibleChildren().ToArray();

            S4JToken firstChild = visibleChildren.GetOrDefault(0);
            S4JToken secondChild = visibleChildren.GetOrDefault(1);
            S4JToken thirdChild = visibleChildren.GetOrDefault(2);
            // S4JToken lastChild = visibleChildren.LastOrDefault();

            if ((firstChild is S4JTokenTextValue && secondChild is S4JTokenParameters) &&
               !(thirdChild is S4JTokenRootObject))
            {
                throw new Exception("Method definition should have object container!");
            }

            // ustalenie root name
            // ustalenie parameters
            else if ((firstChild is S4JTokenTextValue || secondChild is S4JTokenParameters) &&
            thirdChild is S4JTokenRootObject rootObject)
            {
                if (rootObject.GetVisibleChildren().Count() > 1)
                    throw new Exception("Root object should contains only one child!");

                this.IsFullSpecificationProvided = true;

                S4JTokenTextValue nameToken = firstChild as S4JTokenTextValue;
                S4JTokenParameters parametersToken = secondChild as S4JTokenParameters;

                root.Name = "";
                if (nameToken != null)
                {
                    // dodanie tagów do root'a
                    foreach (var tagKV in nameToken.Tags)
                        this.Tags[tagKV.Key] = tagKV.Value;

                    root.Name = UniConvert.ToString(nameToken.ToJson().ParseJsonOrText());
                    root.RemoveChild(nameToken, null);
                }

                root.ParametersDefinitions = new Dictionary<string, S4JFieldDescription>();
                root.ParametersValues = new Dictionary<string, object>();
                if (parametersToken != null)
                {
                    // dodanie tagów do root'a
                    foreach (var tagKV in parametersToken.Tags)
                        this.Tags[tagKV.Key] = tagKV.Value;

                    string lastKey = null;
                    foreach (S4JToken child in parametersToken.Children)
                    {
                        Object val = child.ToJson().ParseJsonOrText();

                        if (child.IsObjectSingleKey)
                        {
                            lastKey = null;
                            root.ParametersDefinitions[UniConvert.ToString(val)] = null;
                            root.ParametersValues[UniConvert.ToString(val)] = null;
                        }
                        else if (child.IsObjectKey)
                        {
                            lastKey = null;
                            lastKey = UniConvert.ToString(val);
                            root.ParametersDefinitions[lastKey] = null;
                            root.ParametersValues[lastKey] = null;
                        }
                        else if (child.IsObjectValue)
                        {
                            if (child is S4JTokenFunction fun)
                            {
                                root.ParametersDefinitions[lastKey] = S4JFieldDescription.Parse(lastKey, fun);
                            }
                            else
                            {
                                root.ParametersDefinitions[lastKey] = S4JFieldDescription.Parse(lastKey, UniConvert.ToString(val));
                            }
                            root.ParametersValues[lastKey] = null;
                        }
                    }
                    root.RemoveChild(parametersToken, null);
                }
            }

            else if (visibleChildren.Any(i => i is S4JTokenParameters))
            {
                throw new Exception("Invalid method definition");
            }

            else if (visibleChildren.Count > 1)
            {
                throw new Exception("Method should have only one visible child");
            }

            return true;
        }
    }
}
