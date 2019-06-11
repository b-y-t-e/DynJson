using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynJson.Helpers.CoreHelpers;

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

method ( osoba : any )  {
/*
q-many( insert into osoba(imie) select @osoba_imie; ),
*/
query( select imie from osoba where imie = 'test_sql' )
}
";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_sql' }" });

            Assert.AreEqual("\"test_sql\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_dynlan_1()
        {
            // await new DbForTest().PrepareDb();

            var script1  = @" 
method ( osoba : any )  {
/*
@-many( item = dictionary(); item.imie = osoba.imie; db.primary.save('osoba', item)  ),
*/
query( select imie from osoba where imie = 'test_dynlan' )
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan' }" });

            Assert.AreEqual("\"test_dynlan\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_dynlan_2()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
@-many( db.primary.save('osoba', osoba)  ),
*/
query( select imie from osoba where imie = 'test_dynlan2' )
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan2' }" });

            Assert.AreEqual("\"test_dynlan2\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_csharp_1()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
@cs( var item = new Dictionary<string, object>(); item[""imie""] = osoba.imie; db.primary.save(""osoba"", item);  ),
*/
query( select imie from osoba where imie = 'test_dynlan_cs' )
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan_cs' }" });

            Assert.AreEqual("\"test_dynlan_cs\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_csharp_2()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
@cs( db.primary.save(""osoba"", osoba)  ),
*/
query( select imie from osoba where imie = 'test_dynlan2' )
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan2' }" });

            Assert.AreEqual("\"test_dynlan2\"", result.ToJson());
        }

        [Test]
        async public Task test_identity_generation()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
@cs( db.primary.save(""osoba"", osoba)  ),
*/
@-many(osoba.ID)
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan2' }" });

            //var id = UniConvert.ToInt64N(result.ToJson());

            Assert.AreNotEqual("null", result.ToJson());
        }

        [Test]
        async public Task test_save_children_csharp()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( dokument : any )  {
/*
@cs( db.primary.save(""dokument"", dokument)  ),
@cs( db.primary.savechildren(""pozycjaDokumentu"", dokument.Pozycje, ""iddokumentu"", dokument.ID)  ),
*/
query(select count(*) from pozycjaDokumentu where iddokumentu = @dokument_id)
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ Numer: 'test', Pozycje: [ {lp : 1}, {lp : 2} ] }" });

            Assert.AreEqual("2", result.ToJson());
        }

        [Test]
        async public Task test_save_children_dynlan()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( dokument : any )  {
/*
@-many( db.primary.save('dokument', dokument)  ),
@-many( db.primary.savechildren('pozycjaDokumentu', dokument.Pozycje, 'iddokumentu', dokument.ID)  ),
*/
query(select count(*) from pozycjaDokumentu where iddokumentu = @dokument_id)
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ Numer: 'test', Pozycje: [ {lp : 1}, {lp : 2} ] }" });

            Assert.AreEqual("2", result.ToJson());
        }
    }
}
