using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Tokens
{
    public class S4JTokenArray : S4JToken
    {
        public S4JTokenArray()
        {
            Children = new List<S4JToken>();
        }

        public override bool BuildJson(StringBuilder Builder)
        {
            if (!IsVisible)
                return false;

            Builder.Append("[");
            Boolean prevWasAdded = false;
            foreach (var child in Children)
            {
                if (prevWasAdded) Builder.Append(",");
                prevWasAdded = child.BuildJson(Builder);
            }
            Builder.Append("]");

            return true;
        }
    }
}
