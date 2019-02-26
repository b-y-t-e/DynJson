using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Classes
{
    public class Sources : Dictionary<string, string>
    {
        public Sources()
        {
         
        }

        public string Get(String Source)
        {
            string val = null;
            this.TryGetValue(Source, out val);
            return val;
        }

        public void Register(String Source, String ConnectionString)
        {
            this[Source] = ConnectionString;
        }
    }
}
