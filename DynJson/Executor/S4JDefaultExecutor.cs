using DynJson.Functions;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DynJson.Executor
{
    public class S4JDefaultExecutor : S4JExecutor
    {
        public S4JDefaultExecutor() :
            base(S4JDefaultStateBag.Get())
        {
            this.TagValidators.Add(context =>
            {
                if (context.Tags.ContainsKey("hidden"))
                    return false;

                return true;
            });

            this.Methods.Add(async (name) =>
            {
                if (name == "test_method_2")
                    return "@@(1+1)";
                return null;
            });
        }
    }
}
