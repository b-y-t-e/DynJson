using DynJson.Functions;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Executor
{
    public class S4JExecutorForTests : S4JDefaultExecutor
    {
        public S4JExecutorForTests() 
        {
            this.Sources.
                Register("primary", "Data Source=.;uid=dba;pwd=sql;initial catalog=testsdb;");

            this.Methods.Add(async (name) =>
            {
                if (name == "test_method_2")
                    return "js(1+1)";
                return null;
            });
        }
    }
}
