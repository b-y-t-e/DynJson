using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenQuotation : S4JToken
    {
        public S4JTokenQuotation()
        {
            IsObjectKey = false;
            Children = new List<S4JToken>();
        }

        public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            //Builder.Append("'");
            base.BuildJson(Builder, Force);
            //Builder.Append("'");

            return true;
        }
        /*public override void AddChildToToken(S4JToken Child)
        {
            // Value = Child;
        }*/

    }
}
