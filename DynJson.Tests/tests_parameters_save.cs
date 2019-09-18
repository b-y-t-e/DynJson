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
q( insert into osoba(imie) select @osoba_imie; ),
*/
query-scalar( select imie from osoba where imie = 'test_sql' )
}
";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_sql' }" });

            Assert.AreEqual("\"test_sql\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_update_js()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
q( delete from osoba where imie = @osoba_imie )
js( 
    item = {
        imie : osoba.imie,
        nazwisko : osoba.nazwisko,
    };
    item = db().save('osoba', item);

    item.nazwisko = 'zmienione_nazwisko';
    db().save('osoba', item, 'imie');
),
*/
q-scalar( select nazwisko from osoba where imie = @osoba_imie )
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_js_to_update', nazwisko: 'test_js_to_update_nazwisko' }" });

            Assert.AreEqual("\"test_js_to_update_nazwisko\"", result.ToJson());
        }

        [Test]
        async public Task test_complex_parameter_save_dynlan_1()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
js( item = new Dictionary(); item.imie = osoba.imie; db().save('osoba', item)  ),
*/
query-scalar( select imie from osoba where imie = 'test_dynlan' )
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
js( db('secondary').save('osoba', osoba)  ),
*/
query( select imie from osoba where imie = 'test_dynlan22' )
}
";
            try
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1, new[] { "{ imie: 'test_dynlan22' }" });

                throw new Exception("Should throw DynLanExecuteException, becouse we dont have 'secondary' connection string");
            }
            catch (InvalidOperationException)
            {
                // ok
            }
           /* catch (DynLan.Exceptions.DynLanExecuteException)
            {
                // ok
            }*/
        }

        [Test]
        async public Task test_complex_parameter_save_dynlan_3()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" 
method ( osoba : any )  {
/*
js( db().save('osoba', osoba)  ),
*/
query-scalar( select imie from osoba where imie = 'test_dynlan2' )
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
cs( var item = new Dictionary<string, object>(); item[""imie""] = osoba.imie; db().save(""osoba"", item);  ),
*/
query-scalar( select imie from osoba where imie = 'test_dynlan_cs' )
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
cs( db().save(""osoba"", osoba)  ),
*/
query-scalar( select imie from osoba where imie = 'test_dynlan2' )
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
cs( db().save(""osoba"", osoba)  ),
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
cs( db().save(""dokument"", dokument)  ),
cs( db().savechildren(""pozycjaDokumentu"", dokument.Pozycje, ""iddokumentu"", dokument.ID)  ),
*/
query-scalar(select count(*) from pozycjaDokumentu where iddokumentu = @dokument_id)
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
js( db().save('dokument', dokument)  ),
js( db().savechildren('pozycjaDokumentu', dokument.Pozycje, 'iddokumentu', dokument.ID)  ),
*/
query-scalar(select count(*) from pozycjaDokumentu where iddokumentu = @dokument_id)
}
";
            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new[] { "{ Numer: 'test', Pozycje: [ {lp : 1}, {lp : 2} ] }" });

            Assert.AreEqual("2", result.ToJson());
        }
    }
}
