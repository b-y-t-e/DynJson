using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenTextValue : S4JToken
    {
        public String Text { get; set; }

        public Object Value { get; set; }

        public String VariableName { get; set; }

        // public Boolean IsVariableReference { get; set; }

        public S4JTokenTextValue()
        {
            Text = "";
            IsObjectKey = false;
            VariableName = null;
            //IsVariableReference = false;
            Children = new List<S4JToken>();
            State = new S4JState()
            {
                StateType = EStateType.S4J_TEXT_VALUE,
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
                this.Text += Char;
            }
        }

        public override bool BuildJson(StringBuilder Builder)
        {
            if (!IsVisible)
                return false;

            if (VariableName != null)
            {
                Builder.Append(Value.SerializeJson());
            }
            else
            {
                //if(Value != null)
                //    Builder.Append(UniConvert.to);
                //else
                Builder.Append(Text);
            }

            return true;
        }

        public override void Commit()
        {
            //this.Text = this.Text.Trim();
            // this.Value = this.Text.DeserializeJson();
            this.IsCommited = true;
            this.CheckVariable();
        }

        public override void MarkAsObjectKey()
        {
            base.MarkAsObjectKey();
            AnalyseValue();
        }

        public override void MarkAsObjectValue()
        {
            base.MarkAsObjectValue();
            AnalyseValue();
        }

        private void CheckVariable()
        {
            if (MyStringHelper.IsVariable(this.Text))
            {
                this.VariableName = this.Text.Trim().Substring(1);
                this.Value = null;
            }
        }

        private void AnalyseValue()
        {
            try
            {
                if (MyStringHelper.IsNumber(this.Text) ||
                    MyStringHelper.IsQuotedText(this.Text))
                {
                    this.Value = this.Text.DeserializeJson();
                }
                else
                {
                    this.Value = this.Text.Trim();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
