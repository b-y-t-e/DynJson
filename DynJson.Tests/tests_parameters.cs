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
            var script1 = @" method ( a : int, b : string!, c: any ) sql( select 1  ) ";

            Assert.ThrowsAsync<S4JNullParameterException>(async () =>
              {
                  var result = await new S4JExecutorForTests().
                      ExecuteWithParameters(script1);
              });
        }

        [Test]
        async public Task test_valid_int_parameter()
        {
            var script1 = @" method ( a : any, b : string!, c: int ) sql( select 1  ) ";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithParameters(script1, 4.1, "", 4.1);
            });
        }

        [Test]
        async public Task test_valid_array_parameter()
        {
            var script1 = @" method ( a : array ) sql( select 1  ) ";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithParameters(script1, "{a:1}");
            });
        }

        [Test]
        async public Task test_valid_object_parameter()
        {
            var script1 = @" method ( a : object ) sql( select 1  ) ";

            Assert.ThrowsAsync<S4JInvalidParameterTypeException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1, "[1,2,3,4,5]");
            });
        }

        [Test]
        async public Task test_isrequired_parameter_json()
        {
            var script1 = @" method ( a : int, b : string!, c: any ) sql( select 1  ) ";

            Assert.ThrowsAsync<S4JNullParameterException>(async () =>
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1);
            });
        }

        [Test]
        async public Task test_array_parameter_json()
        {
            var script1 = @" method ( c: array ) c#( c.Count  ) ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "[1,2,3,4]");

            Assert.AreEqual("4", result.ToJson());
        }

        [Test]
        async public Task test_parameter_validcase()
        {
            var script1 = @" method ( c: int ) c#( c )  ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, 12);

            Assert.AreEqual("12", result.ToJson());
        }

        [Test]
        async public Task test_object_parameter_json()
        {
            var script1 = @" method ( c: object ) c#( c.g  ) ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{g:123}");

            Assert.AreEqual("123", result.ToJson());
        }

        [Test]
        async public Task test_int_parameter_json()
        {
            var script1 = @" method ( a : any, b : string!, c: int ) sql( select @c  ) ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "4.1", "''", "4");

            Assert.AreEqual("4", result.ToJson());
        }

        [Test]
        async public Task test_int_complex_parameter_json()
        {
            var script1 = @" method ( a : any, b : string!, c: int ) sql( select @c + @a_f2  ) ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "{ f1: 1, f2 : 2, f3: 'c' }", "''", "4");

            Assert.AreEqual("6", result.ToJson());
        }

    }
}
