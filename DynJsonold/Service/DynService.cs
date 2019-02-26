using DynJson.Classes;
using DynJson.Exceptions;
using DynJson.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Service
{
    public abstract class DynService
    {
        public Func<DynServiceFindMethodArgs, S4JTokenRoot> FindMethodDelegate { get; set; }

        // public Func<DynServiceFindMethodArgs, S4JToken> ValidateMethodDelegate { get; set; }

        public async Task<S4JToken> ExecuteWithParameters(String MethodName, Tags Tags, params Object[] Parameters)
        {
            S4JToken foundMethod = null;
            using (DynServiceFindMethodArgs args = new DynServiceFindMethodArgs(MethodName, Tags, Parameters))            
                foundMethod = FindMethodDelegate(args);
            
            if (foundMethod == null)
                throw new MethodNotFoundException($"Method {MethodName} was not found");

            return null;
        }
    }

    public class DynServiceFindMethodArgs : IDisposable
    {
        public String MethodName { get; set; }

        public Tags Tags { get; set; }

        public IList<Object> Parameters { get; set; }

        public DynServiceFindMethodArgs(String MethodName, Tags Tags, params Object[] Parameters)
        {
            this.MethodName = MethodName;
            this.Tags = Tags;
            this.Parameters = Parameters;
        }

        public void Dispose()
        {
            Tags = null;
            Parameters = null;
        }
    }
}
