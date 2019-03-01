using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenComment : S4JToken
    {

        public S4JTokenComment()
        {
            IsVisible = false;
            Children = new List<S4JToken>();
        }        
    }
}
