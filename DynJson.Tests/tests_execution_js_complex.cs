using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_execution_js_complex
    {
        [Test]
        async public Task executor_should_understand_dunamicl_fields_and_values()
        {
            var script1 = @"
            { 
                ""docs"" : 
                [
                    {
                        query(select id, numer from [dokument] where numer in ('numer6', 'numer7') order by numer),
                        ""items"" : 
                        [
                            {
                                query(select lp from pozycjadokumentu where iddokumentu = @id order by lp)
                            }
                        ]
                    }
                ]  
            }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""docs"":[{""id"":6,""numer"":""numer6"",""items"":[{""lp"":10},{""lp"":11}]},{""id"":7,""numer"":""numer7"",""items"":[{""lp"":20}]}]}",
                result.ToJson());
        }
    }
}
