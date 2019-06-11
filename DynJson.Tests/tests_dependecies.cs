using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using System.Linq;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Dynamic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using DynJson.Helpers.CoreHelpers;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using DynLan;
using DynLan.Exceptions;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_dependecies
    {
        [Test]
        public void dynlan_should_throw_exceptionn_if_method_is_not_avaiable()
        {
            string code = "db.primary.exec('select 1') ";

            Assert.Throws(typeof(DynLanExecuteException), () =>
            {
                Object result = new Compiler().
                    Compile(code).
                    Eval();
            });
        }
        
        [Test]
        public async Task inner_dynlan_should_throw_exceptionn_if_method_is_not_avaiable()
        {
            var script1 = @" 
@-many( db.primary.exec('select 1')  )

";          
            Assert.ThrowsAsync(typeof(DynLanExecuteException), async () =>
            {
                var result2 = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1);
            });
        }

        [Test]
        public async Task inner_dynlan_should_throw_exceptionn_if_method_is_not_avaiable_version2()
        {
            var script1 = @" 
@-many( item = dictionary(); db.primary.save('osoba', item);  )

";
            Assert.ThrowsAsync(typeof(DynLanExecuteException), async () =>
            {
                var result2 = await new S4JExecutorForTests().
                    ExecuteWithJsonParameters(script1);
            });
        }

        [Test]
        public void deserializer_cs_should_properly_deserialize_outer_lists()
        {
            string json = @"[1,{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01'}]},3,null]";

            dynamic dynObj = JsonToCsDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj.GetType());
        }

        [Test]
        public void deserializer_cs_should_properly_deserialize_inner_lists()
        {
            string json = @"[1,{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01'}]},3,null]";

            dynamic dynObj = JsonToCsDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj[1]["c"].GetType());
        }

        [Test]
        public void deserializer_cs_should_properly_deserialize_inner_lists_version2()
        {
            string json = @"{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01',c:[1,2,3,{a:1}]}]}";

            dynamic dynObj = JsonToCsDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj["c"].GetType());

            Assert.AreEqual(
                typeof(List<object>),
                dynObj["c"][2]["c"].GetType());
        }

        [Test]
        public void deserializer_dynamic_should_properly_deserialize_outer_lists()
        {
            string json = @"[1,{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01'}]},3,null]";

            dynamic dynObj = JsonToDynamicDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj.GetType());
        }

        [Test]
        public void deserializer_dynamic_should_properly_deserialize_inner_lists()
        {
            string json = @"[1,{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01'}]},3,null]";

            dynamic dynObj = JsonToDynamicDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj[1].c.GetType());
        }

        [Test]
        public void deserializer_dynamic_should_properly_deserialize_inner_lists_version2()
        {
            string json = @"{a:1,b:2,c:[6,7,{a:'a',b:'2018-01-01',c:[1,2,3,{a:1}]}]}";

            dynamic dynObj = JsonToDynamicDeserializer.Deserialize(json);

            Assert.AreEqual(
                typeof(List<object>),
                dynObj.c.GetType());

            Assert.AreEqual(
                typeof(List<object>),
                dynObj.c[2].c.GetType());
        }

        [Test]
        public void deserializer_dynamic_should_properly_deserialize_simple_int_as_json()
        {
            string json = @"1";

            dynamic dynObj = JsonToDynamicDeserializer.Deserialize(json);

            Assert.AreEqual(
                1,
                dynObj);
        }
        [Test]
        public async Task parser_method_is_should_work_fine()
        {
            for (var i = 0; i < 10; i++)
            {
                var refs = new List<MetadataReference>{
                MetadataReference.CreateFromFile(typeof(System.Linq.Enumerable).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.RuntimeBinderException).GetTypeInfo().Assembly.Location),
                MetadataReference.CreateFromFile(typeof(System.Runtime.CompilerServices.DynamicAttribute).GetTypeInfo().Assembly.Location)};

                var imports = ScriptOptions.Default.
                    WithImports(
                        "System",
                        "System.Text",
                        "System.Linq",
                        "System.Collections",
                        "System.Collections.Generic").
                    WithReferences(refs);
                Stopwatch st = new Stopwatch();
                st.Start();
                object result = await CSharpScript.EvaluateAsync(@"
        int a = " + i + @";

        public class osoba2{
        public int wiek;
        public osoba2(){
        wiek = 2;
        }
        };

        return a + new osoba2().wiek;", imports);
                st.Stop();
                Assert.AreEqual(2 + i, result);

            }
        }

    }

}
