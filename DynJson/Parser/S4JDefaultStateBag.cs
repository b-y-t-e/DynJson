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
                        i.AddStatesToBag(
                            new CSharpExpandedFunction(),
                            new CSharpFunction(),
                            new DynLanExpandedFunction(),
                            new DynLanManyFunction(),
                            new DynLanSingleFunction(),
                            new DynLanValueFunction(),
                            new TSqlSingleExpandedFunction(),
                            new TSqlValueExpandedFunction(),
                            new TSqlExpandedFunction(),
                            new TSqlManyFunction());
                    }

            return i;//.Clone();
        }
    }
}
