using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Classes
{
    public class Sources : Dictionary<string, string>
    {
        public string DefaultSourceName;
        public Sources()
        {
            DefaultSourceName = "primary";
        }

        public string Get(String Source)
        {
            string val = null;
            if (Source != null)
                this.TryGetValue(Source, out val);
            return val;
        }

        public string GetDefault()
        {
            string val = null;
            if (DefaultSourceName != null)
            this.TryGetValue(DefaultSourceName, out val);
            return val;
        }

        public void Register(String Source, String ConnectionString, Boolean IsDefault = false)
        {
            this[Source] = ConnectionString;
            if (IsDefault)
                this.DefaultSourceName = Source;
        }
    }
}
