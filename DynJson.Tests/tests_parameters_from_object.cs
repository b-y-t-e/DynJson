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
    public class tests_parameters_from_object
    {
        [Test]
        async public Task should_understand_additional_parameter()
        {
            var script1 = @" method ( anyParam: any, a : @(body) ){ @(a) } ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "123", new S4JExecutorParameter("body", "{a:1,b:2}"));

            Assert.AreEqual(@"{""a"":1,""b"":2}", result.ToJson());
        }

        [Test]
        async public Task should_understand_parameter_from_body()
        {
            var script1 = @" method ( anyParam: any, a : @(body.b) ){ @(a) } ";

            var result = await new S4JExecutorForTests().
                ExecuteWithJsonParameters(script1, "123", new S4JExecutorParameter("body", "{a:1,b:2}"));

            Assert.AreEqual(@"2", result.ToJson());
        }
    }
}
