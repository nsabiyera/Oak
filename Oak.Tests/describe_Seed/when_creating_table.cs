using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Data.SqlClient;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class when_creating_table : _seed
    {
        void before_each()
        {
            seed.PurgeDb();
        }

        void act_each()
        {
            seed.CreateTable("Customers", new dynamic[] 
            { 
                seed.Id()
            }).ExecuteNonQuery();

            command = seed.CreateTable("Users", columns);

            command.ExecuteNonQuery();
        }

        void table_with_a_string_column()
        {
            before = () =>
                columns = new[]
                {
                    new { FirstName = "nvarchar(255)" }
                };

            it["the command contains create table with column and type"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [FirstName] nvarchar(255) NULL,
                    )
                ");

            it["table can be queried"] = () =>
                "select FirstName from Users".ExecuteReader();

            it["nulls can be inserted into table"] = () =>
                "insert into Users(FirstName) values(null)".ExecuteNonQuery();

            it["length specification is adhered to"] = 
                expect<SqlException>(() => "insert into Users(FirstName) values('{0}')".With(StringWithLength(300)).ExecuteNonQuery());
                
        }

        void column_has_a_null_definition()
        {
            before = () =>
                columns = new[]
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

            it["table can be queried"] = () =>
                "select FirstName from Users".ExecuteReader();

            it["nulls cannot be inserted into table"] = () =>
                expect<SqlException>(() => "insert into Users(FirstName) values(null)".ExecuteNonQuery());

            it["length specification is adhered to"] =
                expect<SqlException>(() => "insert into Users(FirstName) values('{0}')".With(StringWithLength(300)).ExecuteNonQuery());
        }

        void column_has_default_value()
        {
            before = () =>
                columns = new[]
                {
                    new { FirstName = "nvarchar(255)", Default = "test"  }
                };

            it["the command creates column with default value"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [FirstName] nvarchar(255) NULL DEFAULT('test'),
                    )
                ");
        }

        void table_with_two_columns()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { FirstName = "nvarchar(255)", Default = "test"  },
                    new { LastName = "nvarchar(255)", Default = "test"  }
                };

            it["contains both columns in create script"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [FirstName] nvarchar(255) NULL DEFAULT('test'),
                        [LastName] nvarchar(255) NULL DEFAULT('test'),
                    )
                ");
        }

        void primary_key_column()
        {
            before = () =>
                columns = new[]
                {
                    seed.GuidId()
                };

            it["contains primary key definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] uniqueidentifier NOT NULL, 
                        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC
                        )
                    )
                ");
        }

        void identity_column()
        {
            before = () =>
                columns = new[]
                {
                    new { Id = "int", Identity = true }
                };

            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL IDENTITY(1,1),
                    )
                ");
        }

        void foreign_key_column()
        {
            before = () =>
                columns = new[]
                {
                    new { CustomerId = "int", ForeignKey = "Customers(Id)" }
                };
            
            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [CustomerId] int NULL FOREIGN KEY REFERENCES Customers(Id),
                    )
                ");
        }

        void foreign_key_column_not_null()
        {
            before = () =>
            {
                columns = new[]
                {
                    new { CustomerId = "int", ForeignKey = "Customers(Id)", Nullable = false }
                };
            };

            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [CustomerId] int NOT NULL FOREIGN KEY REFERENCES Customers(Id),
                    )
                ");
        }

        void generating_a_table_where_column_is_an_identity_column_and_primary_key()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                };

            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL IDENTITY(1,1), 
                        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC
                        )
                    )
                ");
        }

        void generating_a_table_with_identity_column_and_another_column()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Name = "nvarchar(255)" }
                };

            it["contains both columns and the primary key constraint"] = () =>
               CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(255) NULL, 
                        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC
                        )
                    )
                ");
        }
    }
}
