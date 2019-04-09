using DynJson.Exceptions;
using DynJson.Executor;
using DynJson.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Classes
{
    public class Methods
    {
        Dictionary<string, S4JToken> methods;

        List<Func<String, Task<String>>> methodGetters;

        //////////////////////////////////

        S4JExecutor executor;

        //////////////////////////////////

        public Methods(S4JExecutor Executor)
        {
            this.executor = Executor;
            this.methods = new Dictionary<string, S4JToken>();
            this.methodGetters = new List<Func<string, Task<string>>>();
        }

        //////////////////////////////////

        public void Add(String Name, String Definition)
        {
            this.methods[Name] = executor.Parse(Definition);
        }

        public void Add(String Name, S4JToken Definition)
        {
            this.methods[Name] = Definition;
        }

        public void Add(Func<string, Task<string>> FunctionGetter)
        {
            this.methodGetters.Add(FunctionGetter);
        }

        //////////////////////////////////

        public async Task<S4JToken> Find(String Name)
        {
            S4JToken method = null;
            methods.TryGetValue(Name, out method);
            if (method != null)
                return method;

            if (methodGetters != null)
            {
                foreach (var methodGetter in methodGetters)
                {
                    string definition = await methodGetter(Name);
                    if (!string.IsNullOrEmpty(definition))
                    {
                        method = this.executor.Parse(definition);
                        methods[Name] = method;
                    }
                }
            }

            if (method != null)
                return method;

            throw new MethodNotFoundException(Name, $"Method {Name} was not found");
        }
    }
}
