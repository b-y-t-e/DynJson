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
    public class tests_nulls
    {
        [Test]
        async public Task null_as_value_is_null_in_array()
        {
            var script1 = @"[ 1, null   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }

        [Test]
        async public Task executed_null_as_value_is_null_in_array()
        {
            var script1 = @"[  1, js(  null  ) expand   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1]",
                result.ToJson());
        }

        [Test]
        async public Task executed_null_as_value_is_null_in_array2()
        {
            var script1 = @"[  1, js(  null  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }


        [Test]
        async public Task null_in_json_object_is_null_in_array2()
        {
            var script1 = @"[ 1, { js(  null  ) }   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1]",
                result.ToJson());

            /*Assert.AreEqual(
                @"[1,null]",
                result.ToJson());*/
        }

        [Test]
        async public Task null_as_value_is_null_in_object()
        {
            var script1 = @"{ a: 1, b: null   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:null}",
                result.ToJson());
        }

        [Test]
        async public Task executed_null_as_value_is_null_in_object()
        {
            var script1 = @"{ a: 1, b: js(  null  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:null}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object()
        {
            var script1 = @"{ a: 1, b: { query-scalar( select null  ) }   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:null}",
                result.ToJson());
        }
        
        [Test]
        async public Task null_in_json_object_is_null_in_object2()
        {
            var script1 = @"{ a: 1, b: { query-scalar( select null  ), c:2 }   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:{c:2}}",
                result.ToJson());
        }


        [Test]
        async public Task null_in_json_object_is_null_in_object22()
        {
            var script1 = @"{  null  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{null}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object3()
        {
            var script1 = @"{  { query-scalar( select null  ) }   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{null}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object4()
        {
            var script1 = @"{ a: 1, b: { js(  null  ) }   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:null}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object5()
        {
            var script1 = @"{ a: 1, b: [ js(  null  ) ]   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:[null]}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object6()
        {
            var script1 = @"{ a: 1, b: [ js(  null  ) ]   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:[null]}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object66()
        {
            var script1 = @"{ a: 1, b: [ js(  null  ) expand ]   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:[]}",
                result.ToJson());
        }

        [Test]
        async public Task null_in_json_object_is_null_in_object7()
        {
            var script1 = @"{ a: 1, b: [ js(  null  ) ]   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:[null]}",
                result.ToJson());
        }
    }
}
