using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using DynJson.Exceptions;
using DynLan.Exceptions;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_api
    {
        [Test]
        async public Task api_call_method_notexists()
        {
            var script1 = @"{ @@(api.exec('test_method')) }";

            try
            {
                var result = await new S4JExecutorForTests().
                    ExecuteWithParameters(script1);

                var txt = result.ToJson();

                throw new Exception("MethodNotFoundException should by thrown!");
            }
            catch (DynLanExecuteException ex)
            {
                if (ex.Message != "Method test_method was not found")
                    throw;
            }
            catch (MethodNotFoundException ex)
            {

            }
        }

        [Test]
        async public Task api_call_method_exists()
        {
            var script1 = @"@@(api.exec('test_method_2'))";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual("2", txt);
        }
    }
}
