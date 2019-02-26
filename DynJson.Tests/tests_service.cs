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
    public class tests_service
    {
        [Test]
        async public Task test_complex_parameter_save_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 

method ( osoba : any ) 
sql( insert into osoba(imie) select @osoba_imie; ),
sql( select imie from osoba where imie = 'test_sql' )


";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ imie: 'test_sql' }");

            Assert.AreEqual("\"test_sql\"", result.ToJson());
        }
    }
}
