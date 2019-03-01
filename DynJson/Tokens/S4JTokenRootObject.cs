using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;

namespace DynJson.Tokens
{
    public class S4JTokenRootObject : S4JToken
    {
        public S4JTokenRootObject()
        {
            Children = new List<S4JToken>();
        }

        public override Dictionary<String, Object> GetParameters()
        {
            return null;
        }

        /*public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            foreach (var child in Children)
                child.BuildJson(Builder, Force);

            return true;
        }*/
    }

}
