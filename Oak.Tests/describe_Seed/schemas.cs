using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using System.Configuration;

namespace Oak.Tests.describe_Seed
{
    public class Table1 : DynamicRepository
    {
        public Table1() : base(null, "Logging.Table1", "Id") { }
    }

    class schemas : _seed
    {
        void for_mssql()
        {
            if(IsSqlCe()) return; //these tests are not applicable for SQLCE

            before = () =>
            {
                seed.PurgeDb();
                seed.CreateSchema("Logging").ExecuteNonQuery();
            };

            it["creates table"] = () =>
            {
                seed.CreateTable("Logging", "Table1", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                new { Name = "name 1" }.InsertInto("Logging.[Table1]");

                var repo = new Table1();

                repo.All().Count().should_be(1);
            };

            it["ads column"] = () =>
            {
                seed.CreateTable("Logging", "Table1", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.AddColumns("Logging", 
                    "Table1", 
                    new { NickName = "nvarchar(255)" }).ExecuteNonQuery();

                new { Name = "name 1", NickName = "nick name" }.InsertInto("Logging.[Table1]");

                var repo = new Table1();

                var firstRecord = repo.All().First();

                repo.All().Count().should_be(1);

                (firstRecord.Name as string).should_be("name 1");

                (firstRecord.NickName as string).should_be("nick name");
            };

            it["drops column"] = () =>
            {
                seed.CreateTable("Logging", "Table1", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.AddColumns("Logging",
                    "Table1",
                    new { NickName = "nvarchar(255)" }).ExecuteNonQuery();

                seed.DropColumn("Logging", "Table1", "Name").ExecuteNonQuery();

                new { NickName = "nick name" }.InsertInto("Logging.[Table1]");

                var repo = new Table1();

                var firstRecord = repo.All().First();

                ((bool)firstRecord.RespondsTo("Name")).should_be(false);
            };

            it["renames column"] = () =>
            {
                seed.CreateTable("Logging", "Table1", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.AddColumns("Logging",
                    "Table1",
                    new { NickName = "nvarchar(255)" }).ExecuteNonQuery();

                new { NickName = "nick name" }.InsertInto("Logging.[Table1]");

                seed.RenameColumn("Logging", "Table1", "NickName", "NickName2").ExecuteNonQuery();

                var repo = new Table1();

                var firstRecord = repo.All().First();

                ((bool)firstRecord.RespondsTo("NickName")).should_be(false);

                ((bool)firstRecord.RespondsTo("NickName2")).should_be(true);
            };

            it["drops constraints"] = () =>
            {
                seed.CreateTable("Logging", "Table1", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.AddColumns("Logging",
                    "Table1",
                    new { NickName = "nvarchar(255)" }).ExecuteNonQuery();

                new { NickName = "nick name" }.InsertInto("Logging.[Table1]");

                seed.DropConstraint("Logging", "Table1", "Id").ExecuteNonQuery();

                seed.DropColumn("Logging", "Table1", "Id").ExecuteNonQuery();
            };
        }

        void for_sqlce()
        {
            if(!IsSqlCe()) return; //these tests are only applicable for SQLCE

            it["gives error that sqlce isn't supported"] = todo;
        }
    }
}
