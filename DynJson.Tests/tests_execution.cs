using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_execution
    {
        [Test]
        async public Task executor_should_understand_dunamicl_fields_and_values()
        {
            var script1 = @"{ ""a"": 1, cs(""bb"") : cs( 999 )  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":1,""bb"":999}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_empty_arguments()
        {
            var script1 = @"  method1 (param1) { { ""a"": cs(param1)}  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":null}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_one_argument()
        {
            var script1 = @"  method1 (param1) { { ""a"": cs(param1) }}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new object[] { 999 });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":999}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_many_arguments()
        {
            var script1 = @"  method1 (param1, param2, param3, param4) { { ""a"": cs(param1+param2+param3+param4) }}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new object[] { 1, 10, 100, 1000.0 });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":1111.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_null_value_forKey()
        {
            var a = 1 + null;

            var script1 = @"   { ""a"": null, ""b"" : cs(1+(int?)a)  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":null,""b"":null}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_dunamicl_fields_and_values_no_quotes()
        {
            var script1 = @"{ a: 1, cs(""bb"") : cs( 999 )  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,""bb"":999}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_class_fields_for_object()
        {
            var script1 = @"{ a: 1, cs( 
    class osoba { public string imie; public string nazwisko; } 
    osoba o = new osoba(); 
    o.imie = ""adam""; 
    o.nazwisko = ""adsafasg""; 
    return o; ) expand }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,""imie"":""adam"",""nazwisko"":""adsafasg""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_simple_function_with_outer_comments()
        {
            var script1 = @"{ b : cs( ""abc"" + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:""abc1""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_simple_function_with_outer_comments_2()
        {
            var script1 = @"{ b : cs( ""abc"" + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:""abc1""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values()
        {
            var script1 = @"{ a: 1, b : cs( a + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values_version2()
        {
            var script1 = @"{ a: 1, b : cs( a + 1 ), c : cs( a + b )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2,c:3}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values_version3()
        {
            var script1 = @"{ a: 1, b : cs( a + 1 ), c : cs( a + b ), d: {a:10, b:cs(a+c)}   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2,c:3,d:{a:10,b:13}}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_fields_for_object()
        {
            var script1 = @"{ a: 1, cs(  var dict = new Dictionary<String, Object>(); dict[""b""] = 2; dict[""c""] = 3; return dict;  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,""b"":2,""c"":3}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_fields_for_object2()
        {
            var script1 = @"{ a: 1, cs(  var dict = new Dictionary<String, Object>(); dict[""b""] = 2; dict[""c""] = 3; return dict;  ) expand  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,""b"":2,""c"":3}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_fields_for_object()
        {
            var script1 = @"{ a: 1, cs(  null  ) expand, d: 3   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,d:3}",
                result.ToJson());
        }

        /*[Test]
        async public Task executor_now()
        {
            var script1 = @"{data:js(DateTime.Now)}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                "{data:\"" + DateTime.Now.Date.ToString(null, System.Globalization.DateTimeStyles.RoundtripKind) + "\"}",
                result.ToJson());
        }*/

        [Test]
        async public Task executor_should_understand_additional_empty_items_for_array()
        {
            var script1 = @"[ 1, cs(  var list = new List<Object>(); return list;  ) expand   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_items_for_array()
        {
            var script1 = @"[ 1, cs(  return null;  ) expand   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_items_for_array2()
        {
            var script1 = @"[ 1, cs(  return null;  )    ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_items_for_array_version2()
        {
            var script1 = @"[ 1, cs(  return 2; )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,2]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_items_for_array_version3()
        {
            var script1 = @"[ 1, cs(  
                var list = new List<Object>();
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 2; 
                    dict[""c""] = 3; 
                    list.Add(dict);
                }
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 22; 
                    dict[""c""] = 33; 
                    list.Add(dict);
                }
                return list;  ) expand   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2,""c"":3},{""b"":22,""c"":33}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_for_array()
        {
            var script1 = @"[ 1, {cs(  
                var list = new List<Object>();
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 2; 
                    dict[""c""] = 3; 
                    list.Add(dict);
                }
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 22; 
                    dict[""c""] = 33; 
                    list.Add(dict);
                }
                return list;  )}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2,""c"":3},{""b"":22,""c"":33}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_with_fields_for_array()
        {
            var script1 = @"[ 1, {cs(  
                var list = new List<Object>();
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 2; 
                    dict[""c""] = 3; 
                    list.Add(dict);
                }
                {
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 22; 
                    dict[""c""] = 33; 
                    list.Add(dict);
                }
                return list;  ),d:100}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2,""c"":3,d:100},{""b"":22,""c"":33,d:100}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_for_array_version2()
        {
            var script1 = @"[ 1, {cs(  
                    var dict = new Dictionary<String, Object>(); 
                    dict[""b""] = 2; 
                    dict[""c""] = 3;                    
                    return dict;  )}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2,""c"":3}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_objects_for_array()
        {
            var script1 = @"[ 1, cs(  
                    null  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_fields_for_object_version2()
        {
            var script1 = @"{ a: 1, b: cs(  return 22;  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:22}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_field_for_object()
        {
            var script1 = @"{ a: 1, b: cs(  null  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,b:null}",
                result.ToJson());
        }

        [Test]
        async public Task executor_simple_csharp_function()
        {
            var script1 = @"{ b : cs( 

        int abc(){
        return 3;
        }

        return abc();

        )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:3}",
                result.ToJson());
        }


    }
}
