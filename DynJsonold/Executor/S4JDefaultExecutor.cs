using DynJson.Functions;
using DynJson.Parser;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Executor
{
    public class S4JDefaultExecutor : S4JExecutor
    {
        public S4JDefaultExecutor() :
            base(S4JDefaultStateBag.Get())
        {
            
        }
    }
}
