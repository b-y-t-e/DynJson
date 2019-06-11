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
    public class tests_parameters
    {
        [Test]
        async public Task test_isrequired_parameter()
        {
            var script1 = @" method ( a : int, b : string!, c: any ){ q-many( select 1  )} ";

            Assert.ThrowsAsync<S4JNullParameterException>(async () =>
              {
                  var result = await new S4JExecutorForTests().
                      ExecuteWithParameters(script1);
              });
        }

        [Test]
        async public Task test_valid_int_parameter()
        {
            var script1 = @" method ( a : any, b : string!, c: int ){ q-many( select 1  )} ";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithParameters(script1, new object[] { 4.1, "", 4.1 });
            });
        }

        [Test]
        async public Task test_valid_array_parameter()
        {
            var script1 = @" method ( a : array ) {q-many( select 1  ) }";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithParameters(script1, new object[] { "{a:1}" });
            });
        }

        [Test]
        async public Task test_valid_object_parameter()
        {
            var script1 = @" method ( a : object ) {q-many( select 1  )} ";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1, new string[] { "[1,2,3,4,5]" });
            });
        }

        [Test]
        async public Task test_isrequired_parameter_json()
        {
            var script1 = @" method ( a : int, b : string!, c: any ){ q-many( select 1  ) }";

            Assert.ThrowsAsync<S4JNullParameterException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1);
            });
        }

        [Test]
        async public Task test_array_parameter_json()
        {
            var script1 = @" method ( c: array ){ @cs( c.Count  )} ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new string[] { "[1,2,3,4]" });

            Assert.AreEqual("4", result.ToJson());
        }

        [Test]
        async public Task test_parameter_validcase()
        {
            var script1 = @" method ( c: int ){ @cs( c ) } ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new object[] { 12 });

            Assert.AreEqual("12", result.ToJson());
        }

        [Test]
        async public Task test_parameter_same_names()
        {
            var script1 = @" 
        method(a:int, b:int, c:int)
        {
            {
            a: @-many(a),
			b: @-many(a + b),
			c: @-many(a + b + c)
                }
        }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new object[] { 1, 2, 3 });

            Assert.AreEqual("{a:null,b:null,c:null}", result.ToJson());
        }


        [Test]
        async public Task test_object_parameter_json()
        {
            var script1 = @" method ( c: object ) {@cs( c.g  ) }";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new string[] { "{g:123}" });

            Assert.AreEqual("123", result.ToJson());
        }

        [Test]
        async public Task test_int_parameter_json()
        {
            var script1 = @" method ( a : any, b : string!, c: int ){ query( select @c  ) }";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new string[] { "4.1", "''", "4" });

            Assert.AreEqual("4", result.ToJson());
        }

        [Test]
        async public Task test_int_complex_parameter_json()
        {
            var script1 = @" method ( a : any, b : string!, c: int ) {query( select @c + @a_f2  ) }";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, new string[] { "{ f1: 1, f2 : 2, f3: 'c' }", "''", "4" });

            Assert.AreEqual("6", result.ToJson());
        }

    }
}
