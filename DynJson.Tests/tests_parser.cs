using Newtonsoft.Json.Linq;
using DynJson.Parser;
using System;
using NUnit;
using NUnit.Framework;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_parser
    {
        [Test]
        public void parser_method_is_should_work_fine()
        {
            var chars = @"{a:'cos'}".ToCharArray();

            Assert.
                False(
                    S4JParserHelper.Is(chars, 0, "'".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 3, "'".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 2, ":'".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 0, "{".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 0, "{a".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 1, "a:'".ToCharArray()));

            Assert.
                False(
                    S4JParserHelper.Is(chars, 0, "{{a".ToCharArray()));

            Assert.
                False(
                    S4JParserHelper.Is(new char[0], 1, "{".ToCharArray()));

            Assert.
                True(
                    S4JParserHelper.Is(chars, 8, "}".ToCharArray()));

            Assert.
                False(
                    S4JParserHelper.Is(chars, 8, "}}}".ToCharArray()));
        }

        [Test]
        public void parser_should_understand_root_name()
        {
            var script1 = @" a( p ) { 1 } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                "a(p){1}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_root_name_and_few_parameters()
        {
            var script1 = @" a( p, c : int, b : string ) { 1 } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                "a(p,c:int,b:string){1}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_invalid_object()
        {
            var script1 = @" { ""a"" } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{""a""}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_invalid_object_version2()
        {
            var script1 = @" { ""a"" , ""b""  } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{""a"",""b""}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_invalid_object_version3()
        {
            var script1 = @" { 11 } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{11}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_invalid_object_version4()
        {
            var script1 = @" { 22 , 33  } ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{22,33}",
                result.ToJson());
        }

        [Test]
        public void parser_should_ignore_comment()
        {
            var script1 = @" 3 /* abc */ ";

            var result = new S4JParserForTests().
                Parse(script1);
            
            Assert.AreEqual(
                "3",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_object()
        {
            var script1 = @" {  a: 1, b: 2, c: 3 } ";

            var result = new S4JParserForTests().
                Parse(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                "{a:1,b:2,c:3}",
                result.ToJson());
        }


        [Test]
        public void parser_should_understand_simple_function()
        {
            var script1 = @"{ b : q-many( select 1 )   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select 1)}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_sql_function_wth_getdate()
        {
            var script1 = @"{ b : q-many( select getdate())   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select getdate())}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_quotation_with_sql_function()
        {
            var script1 = @"{ b : "" q-many( select getdate()   )  "" }";

            var result = new S4JParserForTests().
                Parse(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{b:"" q-many( select getdate()   )  ""}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_function_with_quotation1()
        {
            var script1 = @"{ b : q-many(select abc('def'))   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select abc('def'))}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_function_with_quotation1_inside_quotation1()
        {
            var script1 = @"{ b : q-many(select abc('d\'ef'))   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select abc('d\'ef'))}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_function_with_quotation2_inside_quotation2()
        {
            var script1 = @"{ b : q-many(select abc(""d\""ef""))   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select abc(""d\""ef""))}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_function_with_quotation2_inside_quotation2_version2()
        {
            var script1 = @"{ b : q-many(select abc(""d\""ff\""ef""))   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select abc(""d\""ff\""ef""))}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_function_with_quotation1_inside_quotation2()
        {
            var script1 = @"{ b : q-many(select abc(""d'ff'ef""))   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select abc(""d'ff'ef""))}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_function_with_comments()
        {
            var script1 = @"{ b : q-many( select 1 /* abc */ )   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:q-many(select 1)}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_function_with_outer_comments()
        {
            var script1 = @"{ b : /*q-many( select 1 /* abc */ )*/   }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{b:}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_json_object_test1()
        {
            var script1 = @"{    a : 'cos', b : q-many(select 1 ), c: 'aaa' }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{a:'cos',b:q-many(select 1),c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_json_object_test2()
        {
            var script1 = @"{a : 'cos', q-many( select 1 as val ), c: 'aaa' }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{a:'cos',q-many(select 1 as val),c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_json_array_test1()
        {
            var script1 = @"[1 , 2 , 3 , 'abc' ]";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"[1,2,3,'abc']",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_string_value1()
        {
            var script1 = @" 'ab c ' ";

            var result = new S4JParserForTests().
                Parse(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                "'ab c '",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_simple_double_value1()
        {
            var script1 = @" 4324234.66 ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                "4324234.66",
                result.ToJson());
        }


        [Test]
        public void parser_should_ignore_comment_in_comment()
        {
            var script1 = @" 4324234.66 /* abc /* abc */ abc */ ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                "4324234.66",
                result.ToJson());
        }

        [Test]
        public void parser_should_ignore_comment_inside_table()
        {
            var script1 = @"[1 , 2 , /* /* abc */ */ 3 , 'abc' ]";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"[1,2,3,'abc']",
                result.ToJson());
        }

        [Test]
        public void parser_should_ignore_comment_inside_object()
        {
            var script1 = @"{a : 'cos', /* abc*/  q-many( select 1 as val ), c: 'aaa' }";

            var result = new S4JParserForTests().
                Parse(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{a:'cos',q-many(select 1 as val),c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_inner_object()
        {
            var script1 = @"{a : 'cos', d : { a : 1, b : 2, c : 'abc'}, c: 'aaa' }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{a:'cos',d:{a:1,b:2,c:'abc'},c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_inner_object_and_arrays1()
        {
            var script1 = @"{  d: [ {f : 6} ] , c: 'aaa' }";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{d:[{f:6}],c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_ignore_root_tags()
        {
            var script1 = @"
#tag1 #tag2
{  
    d: [ {f : 6} ] , 
    c: 'aaa' }
";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{d:[{f:6}],c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_ignore_inner_tags()
        {
            var script1 = @"
{  
    #permission:admin
    d: [ {f : 6} ] , 
    c: 'aaa' 
}
";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{d:[{f:6}],c:'aaa'}",
                result.ToJson());
        }


        [Test]
        public void parser_should_ignore_any_tags()
        {
            var script1 = @"
#tag1 #tag2
{  
    #permission:admin
    d: [ {f : 6} ] , 
    c: 'aaa' }
";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{d:[{f:6}],c:'aaa'}",
                result.ToJson());
        }


        [Test]
        public void parser_should_understand_inner_object_and_arrays2()
        {
            var script1 = @"
        {
            a : 'cos', 
            d : { 
                a : 1, 
                b : 2, 
                c : 'abc', 
                d: [1,2,3, {g: 8, f : 6} ]  
            }, 
            c: 'aaa' 
        }
        ";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{a:'cos',d:{a:1,b:2,c:'abc',d:[1,2,3,{g:8,f:6}]},c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_inner_object_and_arrays3()
        {
            var script1 = @"{d:[{f:6}],c:'aaa'}";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                @"{d:[{f:6}],c:'aaa'}",
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_inner_object_and_arrays4()
        {
            var script1 = @"{d:[{f:6}],c:'aaa',d:[{a:['gg']}],e:'b'}";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                script1,
                result.ToJson());
        }

        [Test]
        public void parser_should_understand_inner_object_and_arrays5()
        {
            var script1 = @"[{d:[{a:['gg']}]},9,1]";

            var result = new S4JParserForTests().
                Parse(script1);

            Assert.AreEqual(
                script1,
                result.ToJson());
        }
    }
}
