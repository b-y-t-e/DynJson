using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public enum TokenTextType
    {
        TEXT = 0,
        // VARIABLE_REFERENCE = 1,
        VARIABLE_OUTPUT = 2
    }

    public class S4JTokenTextValue : S4JToken
    {
        public TokenTextType WorkType { get; private set; }

        public String Text { get; set; }
        
        ////////////////////////////////////

        public String VariableName { get; set; }

        // public String OutputVariableName { get; set; }

        ////////////////////////////////////

        public S4JTokenTextValue()
        {
            WorkType = TokenTextType.TEXT;
            Text = "";
            IsObjectKey = false;
            VariableName = null;
            //IsVariableReference = false;
            Children = new List<S4JToken>();
            State = S4JDefaultStateBag.Get().ValueState /* new S4JState()
            {
                StateType = EStateType.S4J_TEXT_VALUE,
                IsValue = true,
            }*/;
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
                this.Text += Char;
            }
        }

        public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            if (VariableName != null)
            {
                Builder.Append(Result.SerializeJson());
            }
            else
            {
                Builder.Append(Text);
            }

            return true;
        }

        public override bool Commit()
        {
            //this.Text = this.Text.Trim();
            // this.Value = this.Text.DeserializeJson();

            this.IsCommited = true;
            this.CheckVariable();

            if (this.WorkType == TokenTextType.VARIABLE_OUTPUT)
            {
                this.PrevToken.OutputVariableName = VariableName;
                this.Parent.RemoveChild(this);
                return false;
            }

            return true;
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
            /*if (MyStringHelper.IsVariable(this.Text))
            {
                this.WorkType = TokenTextType.VARIABLE_REFERENCE;
                this.VariablePath = this.Text.Trim().Substring(1).Trim();
                this.Result = null;
            }
            else*/

            if (MyStringHelper.IsVariableOutput(this.Text))
            {
                this.WorkType = TokenTextType.VARIABLE_OUTPUT;
                this.VariableName = this.Text.Trim().Substring(3).Trim();
                //if (MyStringHelper.IsVariable(this.VariablePath))
                {
                    this.VariableName = this.VariableName.Trim();
                }
                this.Result = null;
            }
        }

        private void AnalyseValue()
        {
            try
            {
                if (MyStringHelper.IsNumber(this.Text) ||
                    MyStringHelper.IsQuotedText(this.Text))
                {
                    this.Result = this.Text.DeserializeJson();
                }
                else
                {
                    this.Result = this.Text.Trim();
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
