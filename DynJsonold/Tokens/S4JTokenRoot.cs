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

        public Dictionary<String, Object> Parameters { get; set; }

        //////////////////////////////////////////////////

        public S4JTokenRoot()
        {
            Children = new List<S4JToken>();
            Parameters = new Dictionary<string, object>();
            ParametersDefinitions = new Dictionary<string, S4JFieldDescription>();
        }

        public override S4JToken Clone()
        {
            S4JTokenRoot token = base.Clone() as S4JTokenRoot;
            token.ParametersDefinitions = new Dictionary<string, S4JFieldDescription>(this.ParametersDefinitions);
            token.Parameters = new Dictionary<string, object>(this.Parameters);
            return token;
        }

        public override void AddChildToToken(S4JToken Child)
        {
            base.AddChildToToken(Child);
        }

        public override Dictionary<String, Object> GetParameters()
        {
            return Parameters;
        }

        public override bool BuildJson(StringBuilder Builder)
        {
            if (!IsVisible)
                return false;

            if (!string.IsNullOrEmpty(Name))
            {
                Builder.Append(Name);

                Builder.Append("(");
                Int32 index = 0;
                if (Parameters != null)
                    foreach (var attr in ParametersDefinitions)
                    {
                        if (index > 0) Builder.Append(",");
                        if (attr.Value == null)
                        {
                            Builder.Append($"{attr.Key}");
                        }
                        else
                        {
                            Builder.Append($"{attr.Key}:{attr.Value.ToJson()}");
                        }
                        index++;
                    }
                Builder.Append(")");
            }

            base.BuildJson(Builder);

            return true;
        }

        public override void Commit()
        {
            base.Commit();

            S4JTokenRoot root = this;

            // ustalenie root name
            if (root.Children.Count > 1 && (root.Children.FirstOrDefault() is S4JTokenTextValue nameToken))
            {
                // dodanie tagów do root'a
                foreach (var tagKV in nameToken.Tags)
                    this.Tags[tagKV.Key] = tagKV.Value;

                root.Name = UniConvert.ToString(nameToken.ToJson().ParseJsonOrText());
                root.RemoveChild(nameToken, null);
            }

            // ustalenie parameters
            if ((root.Children.FirstOrDefault() is S4JTokenParameters parametersToken))
            {
                // dodanie tagów do root'a
                foreach (var tagKV in parametersToken.Tags)
                    this.Tags[tagKV.Key] = tagKV.Value;

                root.ParametersDefinitions = new Dictionary<string, S4JFieldDescription>();
                root.Parameters = new Dictionary<string, object>();

                string lastKey = null;
                foreach (S4JToken child in parametersToken.Children)
                {
                    Object val = child.ToJson().ParseJsonOrText();

                    if (child.IsObjectSingleKey)
                    {
                        lastKey = null;
                        root.ParametersDefinitions[UniConvert.ToString(val)] = null;
                        root.Parameters[UniConvert.ToString(val)] = null;
                    }
                    else if (child.IsObjectKey)
                    {
                        lastKey = null;
                        lastKey = UniConvert.ToString(val);
                        root.ParametersDefinitions[lastKey] = null;
                        root.Parameters[lastKey] = null;
                    }
                    else if (child.IsObjectValue)
                    {
                        root.ParametersDefinitions[lastKey] = S4JFieldDescription.Parse(lastKey, UniConvert.ToString(val));
                        root.Parameters[lastKey] = null;
                    }
                }
                root.RemoveChild(parametersToken, null);
            }
        }
    }
}
