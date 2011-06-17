using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;
using System.Text.RegularExpressions;

namespace Oak.Tests
{
    class describe_Seed : nspec
    {
        Seed seed;
        string command;
        string table;
        string action;
        dynamic[] schema;
        ConnectionProfile connectionProfile;

        void before_each()
        {
            connectionProfile = new ConnectionProfile { ConnectionString = "" };

            seed = new Seed(connectionProfile);
        }

        void when_creating_table()
        {
            act = () => command = seed.CommandFor(table, action, schema);

            context["definition is create command for User table"] = () =>
            {
                before = () =>
                {
                    table = "Users";
                    action = "create";
                };

                context["column with name: FirstName"] = () =>
                {
                    before = () =>
                        schema = new[]
                        {
                            new { FirstName = "nvarchar(255)" }
                        };

                    it["the command contains create table with column and type"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [FirstName] nvarchar(255),
                            )
                        ");
                };

                context["column with not null definition"] = () =>
                {
                    before = () =>
                        schema = new[]
                        {
                            new { FirstName = "nvarchar(255)", Nullable = false }
                        };

                    it["the command creates column as not null"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [FirstName] nvarchar(255) NOT NULL,
                            )
                        ");
                };

                context["column with default value"] = () =>
                {
                    before = () =>
                        schema = new[]
                        {
                            new { FirstName = "nvarchar(255)", Default = "test"  }
                        };

                    it["the command creates column with default value"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [FirstName] nvarchar(255) DEFAULT('test'),
                            )
                        ");
                };

                context["two columns defined"] = () =>
                {
                    before = () =>
                        schema = new dynamic[]
                        {
                            new { FirstName = "nvarchar(255)", Default = "test"  },
                            new { LastName = "nvarchar(255)", Default = "test"  }
                        };

                    it["contains both columns in create script"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [FirstName] nvarchar(255) DEFAULT('test'),
                                [LastName] nvarchar(255) DEFAULT('test'),
                            )
                        ");
                };

                context["column with primary key"] = () =>
                {
                    before = () =>
                        schema = new[]
                        {
                            new { Id = "uniqueidentifier", PrimaryKey = true }
                        };

                    it["contains primary key definition"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [Id] uniqueidentifier, 
                                CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                                (
                                   [Id] ASC
                                )
                            )
                        ");
                };

                context["column marked as identity column"] = () =>
                {
                    before = () =>
                        schema = new[]
                        {
                            new { Id = "int", Identity = true }
                        };

                    it["contains identity definition"] = () =>
                        CommandShouldBe(@"
                            CREATE TABLE [dbo].[Users]
                            (
                                [Id] int IDENTITY(1,1),
                            )
                        ");
                };
            };
        }

        void CommandShouldBe(string expected)
        {
            command.should_be(ToSingleLine(expected));
        }

        string ToSingleLine(string s)
        {
            var single = Regex.Replace(s, @"[ ]{2,}", "");

            single = single.Trim().Replace(Environment.NewLine, "");

            return single;
        }
    }
}
