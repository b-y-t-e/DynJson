using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;
using DynJson.Parser;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    /* public enum TokenTextType
    {
        TEXT = 0,
        // VARIABLE_REFERENCE = 1,
        VARIABLE_OUTPUT = 2,
        TARGET_SOURCE = 3
    }*/

    public class S4JTokenTextValue : S4JToken
    {
        // public TokenTextType WorkType { get; private set; }

        public String Text { get; set; }

        ////////////////////////////////////

        public String VariableNameFromText { get; set; }

        public String TargetSourceFromText { get; set; }

        public String InArrayFromText { get; set; }

        // public String OutputVariableName { get; set; }

        ////////////////////////////////////

        public S4JTokenTextValue()
        {
            // WorkType = TokenTextType.TEXT;
            Text = "";
            IsObjectKey = false;
            VariableNameFromText = null;
            TargetSourceFromText = null;
            InArrayFromText = null;
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

            if (InArrayFromText != null)
            {
                Builder.Append(Result.SerializeJson());
            }
            else if (VariableNameFromText != null)
            {
                Builder.Append(Result.SerializeJson());
            }
            else if (TargetSourceFromText != null)
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
            bool wasExecuted = true;

            List<String> parts = this.Text.Trim().
                Split(new[] { ' ', '\t', '\n', '\r' }).
                Select(p => p.Trim()).
                Where(p => p != "").
                ToList();

            this.IsCommited = true;

            for (var i = 0; i < parts.Count; i += 2)
            {
                if (i == (parts.Count - 1))
                    break;

                if (this.CheckVariable(parts[i], parts[i + 1]))
                    continue;

                if (this.CheckTargetSource(parts[i], parts[i + 1]))
                    continue;

                if (this.CheckInArray(parts[i], parts[i + 1]))
                    continue;

                break;
            }

            if (!string.IsNullOrEmpty(VariableNameFromText)) // this.WorkType == TokenTextType.VARIABLE_OUTPUT)
            {
                this.PrevToken.OutputVariableName = VariableNameFromText;
                wasExecuted = false;
            }

            if (!string.IsNullOrEmpty(TargetSourceFromText)) // (this.WorkType == TokenTextType.TARGET_SOURCE)
            {
                this.PrevToken.TargetSource = TargetSourceFromText;
                wasExecuted = false;
            }

            if (!string.IsNullOrEmpty(InArrayFromText)) // (this.WorkType == TokenTextType.TARGET_SOURCE)
            {
                this.PrevToken.InArray = true;
                wasExecuted = false;
            }

            if (!wasExecuted)
                this.Parent.RemoveChild(this);

            return wasExecuted;
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

        private bool CheckVariable(String OperatorText, String VariableNameText) // List<String> Parts)
        {
            if (!MyStringHelper.IsVariableOutput(OperatorText, VariableNameText))
                return false;

            //this.WorkType = TokenTextType.VARIABLE_OUTPUT;
            this.VariableNameFromText = VariableNameText;
            this.Result = null;
            return true;
        }

        private bool CheckTargetSource(String OperatorText, String TargetSourceText) // List<String> Parts)
        {
            if (!MyStringHelper.IsTargetSource(OperatorText, TargetSourceText))
                return false;

            //this.WorkType = TokenTextType.TARGET_SOURCE;
            this.TargetSourceFromText = TargetSourceText;
            this.Result = null;
            return true;
        }
        
        private bool CheckInArray(String OperatorText, String InArrayText) // List<String> Parts)
        {
            if (!MyStringHelper.CheckInArray(OperatorText, InArrayText))
                return false;

            //this.WorkType = TokenTextType.TARGET_SOURCE;
            this.InArrayFromText = InArrayText;
            this.Result = null;
            return true;
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
