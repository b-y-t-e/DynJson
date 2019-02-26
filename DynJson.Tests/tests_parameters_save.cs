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
    public class tests_parameters_save
    {
        [Test, Order(-1)]
        async public Task prepare_db()
        {
            await new DbForTest().PrepareDb();
        }

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

        [Test]
        async public Task test_complex_parameter_save_dynlan_1()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any ) 
dynlan( item = dictionary(); item.imie = osoba.imie; db.sql.save('osoba', item)  ),
sql( select imie from osoba where imie = 'test_dynlan' )

";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ imie: 'test_dynlan' }");

            Assert.AreEqual("\"test_dynlan\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_dynlan_2()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any ) 
dynlan( db.sql.save('osoba', osoba)  ),
sql( select imie from osoba where imie = 'test_dynlan2' )

";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ imie: 'test_dynlan2' }");

            Assert.AreEqual("\"test_dynlan2\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_csharp_1()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any ) 
c#( var item = new Dictionary<string, object>(); item[""imie""] = osoba.imie; db.sql.save(""osoba"", item);  ),
sql( select imie from osoba where imie = 'test_dynlan_cs' )

";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ imie: 'test_dynlan_cs' }");

            Assert.AreEqual("\"test_dynlan_cs\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_csharp_2()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any ) 
c#( db.sql.save(""osoba"", osoba)  ),
sql( select imie from osoba where imie = 'test_dynlan2' )

";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ imie: 'test_dynlan2' }");

            Assert.AreEqual("\"test_dynlan2\"", result.ToJson());
        }
    }
}
