using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenObjectContent : S4JToken
    {
        public String Text { get; set; }

        // public Object Value { get; set; }

        public S4JTokenObjectContent()
        {
            Text = "";
            IsObjectKey = false;
            Children = new List<S4JToken>();
        }

        public override Dictionary<String, Object> GetParameters()
        {
            if (Result is IDictionary<string, object> dict)
            {
                Dictionary<String, Object> variables = new Dictionary<string, object>();
                foreach (var item in dict)
                    variables[item.Key] = item.Value;
                return variables;
            }

            return null;
        }

        public override void AddChildToToken(S4JToken Child)
        {
            // Value = Child;
        }

        public override void AppendCharsToToken(IList<Char> Chars)
        {
            foreach (var Char in Chars)
            {
                if (this.Text.Length == 0 && System.Char.IsWhiteSpace(Char))
                    continue;
                this.Text += Char;
            }
        }

        public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            Builder.Append(Text);

            return true;
        }

        public override bool Commit()
        {
            this.Text = this.Text.Trim();
            // this.ValueFromText = this.Text.DeserializeJson();
            base.Commit();
            return true;
        }

    }
}
