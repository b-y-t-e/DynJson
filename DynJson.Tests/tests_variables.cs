using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using DynJson.Tokens;
using System.Threading.Tasks;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_variables
    {
        [Test, Order(-1)]
        async public Task prepare_db()
        {
            await new DbForTest().PrepareDb();
        }

        [Test]
        async public Task test_simple_variable_from_parameter()
        {
            var script1 = @" 
method ( a : any ) 
{
   { js(a)}
}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 1 });

            Assert.AreEqual("{1.0}", result.ToString());
        }

        [Test]
        async public Task test_simple_variables_from_parameter()
        {
            var script1 = @" 
method ( a : any ) 
{
[
    js(a) as b,
    js(b+1)
]
}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 1 });

            Assert.AreEqual("[1.0,2.0]", result.ToString());
        }

        [Test]
        async public Task test_simple_object_value_variable_as_parameter()
        {
            var script1 = @" 
method ( a : any ) 
{
    {variable : js(a)}
}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 1 });

            Assert.AreEqual("{variable:1.0}", result.ToString());
        }

        [Test]
        async public Task test_complex_variable_as_parameter()
        {
            var script1 = @" 
method ( a : any ) 
{
    js(a.imie)
}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { new osoba() { imie = "andrzej" } });

            Assert.AreEqual(@"""andrzej""", result.ToString());
        }

        /*[Test]
        async public Task test_variable_from_inner_result()
        {
            var script1 = @" 

method ( numer : string ) 

    #hidden
    q-many( select * from dokument where numer = @numer ) as @dokument,
    @dokument[0].numer


";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, "numer1");

            Assert.AreEqual(@"{""numer1""}", result.ToString());
        }*/

        [Test]
        async public Task test_variable_from_inner_object()
        {
            var script1 = @" 

method ( numer : string ) 
{
    /*
    { query( select numer, rodzaj from dokument where numer = @numer ) } as dokument,
    */
    js(dokument)
}

";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { "numer1" });

            Assert.AreEqual(@"{""numer"":""numer1"",""rodzaj"":0}", result.ToString());
        }


        [Test]
        async public Task test_variable_from_inner_objects()
        {
            var script1 = @" 

method ( numer : string ) 
{
    /*
    q( select numer, rodzaj from dokument where numer = @numer ) as documents,
    */
    js(documents[0])
}

";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { "numer1" });

            Assert.AreEqual(@"{""numer"":""numer1"",""rodzaj"":0}", result.ToString());
        }

    }
}
