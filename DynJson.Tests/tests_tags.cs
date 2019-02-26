using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using System.Linq;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using DynJson.Tokens;
using System.Threading.Tasks;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_tags
    {
        [Test]
        async public Task test_simple_tags()
        {
            var script1 = @" 
#post #get
method ( a : int, b : string!, c: any ) 
'ok'
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;

            Assert.AreEqual(2, result.Tags.Count);

            if (!result.Tags.ContainsKey("post"))
                throw new Exception("Tag 'post' is missing");
            
            if (!result.Tags.ContainsKey("get"))
                throw new Exception("Tag 'get' is missing");
        }

        [Test]
        async public Task test_simple_tags_with_nospaces()
        {
            var script1 = @" 
(#post)(#get)
method ( a : int, b : string!, c: any ) 
'ok'
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;

            Assert.AreEqual(2, result.Tags.Count);

            if (!result.Tags.ContainsKey("post"))
                throw new Exception("Tag 'post' is missing");

            if (!result.Tags.ContainsKey("get"))
                throw new Exception("Tag 'get' is missing");
        }

        [Test]
        async public Task test_simple_value_tag()
        {
            var script1 = @" 
#permission:admin
method ( a : int, b : string!, c: any ) 
'ok'
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;

            Assert.AreEqual(1, result.Tags.Count);

            Assert.AreEqual("admin", result.Tags["permission"]);
        }

        [Test]
        async public Task test_simple_inner_tag()
        {
            var script1 = @" 
{
    a : 1, 
    #tagb
    b : 2,
    c: 3
}
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;
            
            Assert.AreEqual(1, result[0][2].Tags.Count);

            Assert.AreEqual("tagb", result[0][2].Tags.Keys.FirstOrDefault());
        }

        [Test]
        async public Task test_simple_inner_tag2()
        {
            var script1 = @" 
{
    a : 1, 
    b : 2,
    #tagb
    c: 3
}
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;

            Assert.AreEqual(1, result[0][4].Tags.Count);

            Assert.AreEqual("tagb", result[0][4].Tags.Keys.FirstOrDefault());
        }

        [Test]
        async public Task test_simple_inner_tag3()
        {
            var script1 = @" 
{
    a : 1, 
    b : 2,
    c: 3,
    #tagb
}
";

            var result = new S4JParserForTests().
                Parse(script1) as S4JTokenRoot;

            Assert.AreEqual(6, result[0].Children.Count);

            Assert.AreEqual("{a:1,b:2,c:3}", result.ToJson());
        }


        [Test]
        async public Task test_tag_logic_for_array()
        {
            var script1 = @" 
[
    1,
    #notvisible
    2,
    3,
    4
]
";

            var executor = new S4JExecutorForTests();
            executor.TagValidator = (context) =>
            {
                return false;
            };

            var result = await executor.
                ExecuteWithParameters(script1);
            
            Assert.AreEqual("[1,3,4]", result.ToJson());
        }

        [Test]
        async public Task test_tag_logic_for_object()
        {
            var script1 = @" 
{
    a: 1,
    #notvisible
    b: 2,
    c: 3,
    d: 4
}
";

            var executor = new S4JExecutorForTests();
            executor.TagValidator = (context) =>
            {
                return false;
            };

            var result = await executor.
                ExecuteWithParameters(script1);

            Assert.AreEqual("{a:1,c:3,d:4}", result.ToJson());
        }

        [Test]
        async public Task test_tag_logic_for_object2()
        {
            var script1 = @" 
{
    a: 1,
    #notvisible
    b,
    c: 3,
    d: 4
}
";

            var executor = new S4JExecutorForTests();
            executor.TagValidator = (context) =>
            {
                return false;
            };

            var result = await executor.
                ExecuteWithParameters(script1);

            Assert.AreEqual("{a:1,c:3,d:4}", result.ToJson());
        }
    }
}
