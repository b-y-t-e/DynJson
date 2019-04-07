using Microsoft.CodeAnalysis.CSharp.Scripting;
using Newtonsoft.Json.Linq;
using DynJson.Parser;
using DynJson.Executor;
using System;
using NUnit;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DynJson.tests
{
    [TestFixture]
    public class tests_execution_sql
    {
        [Test, Order(-1)]
        async public Task prepare_db()
        {
            await new DbForTest().PrepareDb();
        }

        [Test]
        async public Task executor_should_understand_simple_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" sql( select 1  ) ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"1",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_parameters_in_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" method(param1) {sql( select @param1 + 1  )} ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { 199 });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"200",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_object_parameter_in_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" method(param1){ sql( select @param1_imie + '!' + @param1_nazwisko  ) }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { new osoba() { imie = "IMIE", nazwisko = "NAZWISKO" } });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"""IMIE!NAZWISKO""",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_list_in_object_parameter_in_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" method(param1) { sql( select @param1_imie + '!' + @param1_nazwisko + '!' + cast((select count(*) from @param1_rodzice) as varchar(max))  ) }";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1, new[] { new osobaWithList() { imie = "IMIE", nazwisko = "NAZWISKO", rodzice = new List<osoba>() { new osoba() { imie = "tata" }, new osoba() { imie = "mama" } } } });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"""IMIE!NAZWISKO!2""",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_list_parameter_in_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" method(param1){ sql( select count(*) from @param1  )} ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(
                    script1,
                    new[] {
                        new List<osoba>() {
                            new osoba() { imie = "IMIE1", nazwisko = "NAZWISKO2" },
                            new osoba() { imie = "IMIE1", nazwisko = "NAZWISKO2" }
                        }
                    });

            var txt = result.ToJson();

            Assert.AreEqual(
                @"2",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_object_from_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" {sql( select imie, nazwisko from osoba where imie = 'imie1' )} ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""imie"":""imie1"",""nazwisko"":""nazwisko1""}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_object_with_inner_fields_from_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" {sql( select imie, nazwisko, idrodzica from osoba where imie = 'imie1' ), ""parent"": { sql(select imie from osoba where id = @idrodzica) } } ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"{""imie"":""imie1"",""nazwisko"":""nazwisko1"",""idrodzica"":3,""parent"":{""imie"":""imie rodzica""}}",
                result.ToJson());
        }

        [Test]
        async public Task executor_should_understand_objects_from_sql()
        {
            // await new DbForTest().PrepareDb();

            var script1 = @" [{sql( select imie, nazwisko from osoba where idrodzica is not null )}] ";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script1);

            var txt = result.ToJson();

            Assert.AreEqual(
                @"[{""imie"":""imie1"",""nazwisko"":""nazwisko1""},{""imie"":""imie2"",""nazwisko"":""nazwisko2""}]",
                result.ToJson());
        }
    }

    public class DbForTest
    {
        private static object lck = new object();

        public async Task PrepareDb()
        {
            var script = @"
[
    sql(

        begin transaction

        if object_id('dbo.dokument') is not null  
            drop table dbo.dokument

        if object_id('dbo.osoba') is not null  
            drop table dbo.osoba

        if object_id('dbo.osoba') is null  
            create table dbo.osoba(
                id int identity(1,1), 
                idrodzica int,
                imie varchar(max), 
                nazwisko nvarchar(max), 
                wiek int, 
                dataurodzenia datetime, 
                utworzono datetime default(getdate())
            )

        if object_id('dbo.dokument') is null  
            create table dbo.dokument(
                id int identity(1,1), 
                numer varchar(max),
                rodzaj int
            )

        if object_id('dbo.pozycjaDokumentu') is null  
            create table dbo.pozycjaDokumentu(
                id int identity(1,1), 
                idDokumentu int,
                lp int 
            )

        commit

    ),

    sql(

        begin transaction

        delete from dbo.dokument

        insert into dbo.dokument(numer, rodzaj)
        select 'numer1', 0

        insert into dbo.dokument(numer, rodzaj)
        select 'numer2', 0

        insert into dbo.dokument(numer, rodzaj)
        select 'numer3', 0

        insert into dbo.dokument(numer, rodzaj)
        select 'numer4', 1

        insert into dbo.dokument(numer, rodzaj)
        select 'numer5', 1

        delete from dbo.osoba

        insert into dbo.osoba(imie, nazwisko, wiek, dataurodzenia, utworzono)
        select 'imie1', 'nazwisko1', 20, '2000-01-01', getdate();

        insert into dbo.osoba(imie, nazwisko, wiek, dataurodzenia, utworzono)
        select 'imie2', 'nazwisko2', 30, '1990-01-01', getdate();

        insert into dbo.osoba(imie, nazwisko, wiek, dataurodzenia, utworzono)
        select 'imie rodzica', 'nazwisko rodzica', 50, '1970-01-01', getdate();

        update osoba set idrodzica = scope_identity() where imie <> 'imie rodzica';

        commit
    )
]
";

            var result = await new S4JExecutorForTests().
                ExecuteWithParameters(script);

        }
    }

    class osoba
    {
        public string imie;
        public string nazwisko;
    }

    class osobaWithList
    {
        public string imie;
        public string nazwisko;
        public List<osoba> rodzice = new List<osoba>();
    }

}
