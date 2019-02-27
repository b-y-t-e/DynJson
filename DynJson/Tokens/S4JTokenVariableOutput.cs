using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenVariableOutput : S4JToken
    {
        public String VariableName { get; set; }

        public S4JTokenVariableOutput()
        {
            VariableName = "";
            IsObjectKey = false;
            Children = new List<S4JToken>();
            State = new S4JState()
            {
                StateType = EStateType.S4J_VARIABLE_OUTPUT,
                IsValue = true,
            };
        }

        public override Dictionary<String, Object> GetParameters()
        {
            return null;
        }

        public override void AddChildToToken(S4JToken Child)
        {

        }

        public override void AppendCharsToToken(IList<Char> Chars)
        {
            foreach (var Char in Chars)
            {
                //if (this.Text.Length == 0 && System.Char.IsWhiteSpace(Char))
                //    continue;
                this.VariableName += Char;
            }
        }

        public override bool BuildJson(StringBuilder Builder)
        {
            if (!IsVisible)
                return false;

            //if(Value != null)
            //    Builder.Append(UniConvert.to);
            //else
            Builder.Append(VariableName);

            return true;
        }

        public override bool Commit()
        {
            //this.Text = this.Text.Trim();
            // this.Value = this.Text.DeserializeJson();
            this.IsCommited = true;
            return true;
        }
    }
}
