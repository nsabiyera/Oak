using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests.describe_Seed
{
    public class Table1 : DynamicRepository
    {
        public Table1() : base(null, "Logging.Table1", "Id") { }
    }

    [Tag("wip")]
    class schemas : _seed
    {
        void specify_create_table()
        {
            seed.PurgeDb();

            seed.CreateSchema("Logging").ExecuteNonQuery();

            seed.CreateTable("Logging", "Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            new { Name = "name 1" }.InsertInto("Logging.[Table1]");

            var repo = new Table1();

            repo.All().Count().should_be(1);
        }

        void specify_add_column()
        {
            seed.PurgeDb();

            seed.CreateSchema("Logging").ExecuteNonQuery();

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
        }

        void specify_drop_column()
        {
            seed.PurgeDb();

            seed.CreateSchema("Logging").ExecuteNonQuery();

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
        }

        void specify_rename_column()
        {
            seed.PurgeDb();

            seed.CreateSchema("Logging").ExecuteNonQuery();

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
        }

        void specify_drop_constraint()
        {
            seed.PurgeDb();

            seed.CreateSchema("Logging").ExecuteNonQuery();

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
        }
    }
}
