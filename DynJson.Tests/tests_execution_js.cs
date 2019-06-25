using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_execution_js
    {
        [Test]
        async public Task executor_should_understand_dunamicl_fields_and_values()
        {
            var script1 = @"{ ""a"": 1, js('bb') : js( 999 )  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":1,""bb"":999.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_empty_arguments()
        {
            var script1 = @"  method1 (param1) { {""a"": js-many(param1)} }";

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
            var script1 = @"  method1 (param1){ { ""a"": js(param1) }}";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 999 });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":999.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_many_arguments()
        {
            var script1 = @"  method1 (param1, param2, param3, param4) { { ""a"": js(param1+param2+param3+param4) } }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 1, 10, 100, 1000.0 });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":1111.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_null_value_forKey()
        {
            var a = 1 + null;

            var script1 = @"   { ""a"": null, ""b"" : js(1+a)  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""a"":null,""b"":1.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_dunamicl_fields_and_values_no_quotes()
        {
            var script1 = @"{ a: 1, js('bb') : js( 999 )  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,""bb"":999.0}",
                result.ToJson());
        }

        /*[Test]
        async public Task executor_should_understand_additional_class_fields_for_object()
        {
            var script1 = @"{ a: 1, js-many( 
class osoba() { imie= ''; nazwisko = ''; } o = osoba(); o.imie = 'adam'; o.nazwisko = 'adsafasg'; return o;
)  }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:1,""imie"":""adam"",""nazwisko"":""adsafasg""}",
                result.ToJson());
        }*/

        [Test]
        async public Task executor_should_understand_simple_function_with_outer_comments()
        {
            var script1 = @"{ b : js( 'abc' + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:""abc1""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_simple_function_with_outer_comments_2()
        {
            var script1 = @"{ b : js( 'abc' + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:""abc1""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values()
        {
            var script1 = @"{ a: 1, b : js( a + 1 )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values_version2()
        {
            var script1 = @"{ a: 1, b : js( a + 1 ), c : js( a + b )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2.0,c:3.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parent_values_version3()
        {
            var script1 = @"{ a: 1, b : js( a + 1 ), c : js( a + b ), d: {a:10, b:js(a+c)}   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:2.0,c:3.0,d:{a:10,b:13.0}}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_fields_for_object()
        {
            var script1 = @"{ a: 1, js-fit(  dict = new dictionary(); dict.b = 2; dict.c = 3; dict;  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,""b"":2.0,""c"":3.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_fields_for_object()
        {
            var script1 = @"{ a: 1, js-fit(  null  ), d: 3   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,d:3}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_items_for_array()
        {
            var script1 = @"[ 1, js-fit(  result = new list(); result.Add(2); result.Add(3); result;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,2.0,3.0]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_empty_items_for_array()
        {
            var script1 = @"[ 1, js-many(  var ListOfString = System.Collections.Generic.List(System.String); result = new ListOfString(); result;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);
            
            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,[]]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_empty_items_for_array3()
        {
            var script1 = @"[ 1, js-many(  result = new list(); result;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,[]]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_empty_items_for_array2()
        {
            var script1 = @"[ 1, js-fit(  result = new list(); result;  )   ]";

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
            var script1 = @"[ 1, js-many(  null;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_items_for_array2()
        {
            var script1 = @"[ 1, js-fit(  null;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[1]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_items_for_array_version2()
        {
            var script1 = @"[ 1, js-fit(  dict = new dictionary(); dict['b'] = 2; dict['c'] = 3; dict;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,2.0]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_items_for_array_version3()
        {
            var script1 = @"[ 1, js-fit(  
                result = new list();
                {
                    dict = new dictionary(); 
                    dict['b'] = 2; 
                    dict['c'] = 3; 
                    result.Add(dict);
                }
                {
                    dict = new dictionary(); 
                    dict['b'] = 22; 
                    dict['c'] = 33; 
                    result.Add(dict);
                }
                result;  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,2.0,22.0]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_for_array()
        {
            var script1 = @"[ 1, {js-fit(  
                lista = new list();
                {
                    dict = new dictionary(); 
                    dict['b'] = 2; 
                    dict.c = 3; 
                    lista.Add(dict);
                }
                {
                    dict = new dictionary(); 
                    dict.b = 22; 
                    dict['c'] = 33; 
                    lista.Add(dict);
                }
                lista;  )}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2.0,""c"":3.0},{""b"":22.0,""c"":33.0}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_with_fields_for_array()
        {
            var script1 = @"[ 1, {js-fit(  
                lista = new list();
                {
                    dict = new dictionary(); 
                    dict['b'] = 2; 
                    dict['c'] = 3; 
                    lista.Add(dict);
                }
                {
                    dict = new dictionary(); 
                    dict['b'] = 22; 
                    dict['c'] = 33; 
                    lista.Add(dict);
                }
                lista;  ),d:100}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2.0,""c"":3.0,d:100},{""b"":22.0,""c"":33.0,d:100}]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_objects_for_array_version2()
        {
            var script1 = @"[ 1, {js-fit(  
                    dict = new dictionary(); 
                    dict['b'] = 2; 
                    dict['c'] = 3;                    
                    dict;  )}   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,{""b"":2.0,""c"":3.0}]",
                result.ToJson());
        }
        
        [Test]
        async public Task executor_should_understand_additional_null_objects_for_array()
        {
            var script1 = @"[ 1, js-many(  
                    null  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1,null]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_objects_for_array1()
        {
            var script1 = @"[ 1, js-fit(  
                    null  )   ]";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"[1]",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_fields_for_object_version2()
        {
            var script1 = @"{ a: 1, b: js-fit(  dict = new dictionary(); dict['bb'] = 22; dict['cc'] = 33; dict;  )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{a:1,b:22.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_additional_null_field_for_object()
        {
            var script1 = @"{ a: 1, b: js-many(  null  )   }";

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
            var script1 = @"{ b : js( 

function abc(){
  return 3
        }
abc()

        )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:3.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_simple_db_query()
        {
            var script1 = @"{ b : js( 

                db().value('select 1')

        )   }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:1.0}",
                result.ToJson());
        }

        [Test]
        async public Task executor_simple_db_querydyn()
        {
            var script1 = @"{ b : js( 

                q = sqlbuilder();
                q.append('select 1');
                db().value(q);
            )
           }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            Assert.AreEqual(
                @"{b:1.0}",
                result.ToJson());
        }
    }
}
