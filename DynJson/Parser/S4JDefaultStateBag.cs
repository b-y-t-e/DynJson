using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using DynJson.Functions;

namespace DynJson.Parser
{
    public static class S4JDefaultStateBag
    {
        static Object lck = new Object();
        static S4JStateBag i;
        public static S4JStateBag Get()
        {
            

            if (i == null)
                lock (lck)
                    if (i == null)
                    {
                        i = new S4JStateBag();
                        i.AddStates(
                            new CSharpFunction(),
                            new JsFunction(),
                            new TSqlSingleExpandedFunction(),
                            new TSqlValueExpandedFunction(),
                            new TSqlManyFunction());
                    }

            return i;//.Clone();
        }
    }
}
