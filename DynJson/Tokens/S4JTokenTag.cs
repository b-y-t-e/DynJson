using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DynJson.Helpers;
using DynJson.Helpers.CoreHelpers;

namespace DynJson.Tokens
{
    public class S4JTokenTag : S4JToken
    {
        public S4JTokenTag()
        {
            Children = new List<S4JToken>();
        }

        public override void Commit()
        {
            base.Commit();


            if (this.Parent == null)
                return;

            string lastKey = null;

            foreach (S4JToken child in this.Children)
            {
                Object val = child.ToJson().ParseJsonOrText();

                if (child.IsObjectKey)
                {
                    lastKey = null;
                    lastKey = UniConvert.ToString(val);
                    this.Tags[lastKey] = null;
                }
                else if (child.IsObjectValue)
                {
                    this.Tags[lastKey] = UniConvert.ToString(val);
                }
                else
                {
                    lastKey = null;
                    this.Tags[UniConvert.ToString(val)] = null;
                }
            }
        }
    }



}
