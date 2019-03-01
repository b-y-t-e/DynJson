using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenFunctionComment : S4JToken
    {
        public S4JTokenFunctionComment()
        {
            IsVisible = false;
            Children = new List<S4JToken>();
        }

        public override void AddChildToToken(S4JToken Child)
        {

        }

    }
}
