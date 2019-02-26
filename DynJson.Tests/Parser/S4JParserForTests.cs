using DynJson.Functions;
using DynJson.Tokens;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynJson.Parser
{
    public class S4JParserForTests : S4JParser
    {
        public S4JParserForTests()
        {
        }

        public S4JTokenRoot Parse(String Text)
        {
            S4JStateBag stateBag = S4JDefaultStateBag.Get();
            return Parse(Text, stateBag);
        }
    }
}
