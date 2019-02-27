using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Executor;

namespace DynJson.Tokens
{
    public class S4JTokenFunction : S4JToken
    {
        public IEvaluator Evaluator { get; set; }

        public Boolean IsEvaluated { get; set; }
        
        ////////////////////////////////////////////

        public S4JTokenFunction()
        {
            Children = new List<S4JToken>();
        }

        public override Dictionary<String, Object> GetParameters()
        {
            return null;
            // throw new NotImplementedException();
        }

        public override bool BuildJson(StringBuilder Builder, Boolean Force)
        {
            if (!IsVisible && !Force)
                return false;

            if (IsEvaluated)
            {
                foreach (var child in Children)
                    child.BuildJson(Builder, Force);
            }
            else
            {
                base.BuildJson(Builder, Force);
            }

            return true;
        }
    }
}
