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
            Sources.Register("primary", "Data Source=?;uid=?;pwd=?;initial catalog=?;");
        }
    }
}
