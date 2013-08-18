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
                seed.Id(),
                new { Name = "nvarchar(255)" }
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
                        [FirstName] nvarchar(255) NULL
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
                        [FirstName] nvarchar(255) NOT NULL
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
                columns = new dynamic[]
                {
                    new { FirstName = "nvarchar(255)", Default = "test", Nullable = false  },
                    new { LastName = "nvarchar(255)" }
                };

            act = () => "insert into Users(LastName) values('not first name')".ExecuteNonQuery();

            it["the command creates column with default value"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [FirstName] nvarchar(255) NOT NULL DEFAULT('test'),
                        [LastName] nvarchar(255) NULL
                    )
                ");

            it["default value is set"] = () =>
                "select FirstName from Users".ExecuteScalar().should_be("test");
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
                        [LastName] nvarchar(255) NULL DEFAULT('test')
                    )
                ");

            it["both columns are selectable"] = () =>
                "select FirstName, LastName from Users".ExecuteReader();
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

            it["it doesn't allow duplicate guid entries"] = () =>
            {
                try
                {
                    var id = Guid.NewGuid().ToString();

                    "insert into Users(Id) values('{0}')".With(id).ExecuteNonQuery();

                    "insert into Users(Id) values('{0}')".With(id).ExecuteNonQuery();

                    throw new InvalidOperationException("Sql exception was not thrown");
                }
                catch (SqlException) { }
            };
        }

        void identity_column()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "int", Identity = true },
                    new { Name = "nvarchar(255)" }
                };

            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(255) NULL
                    )
                ");

            it["inserts auto increment id column"] = () =>
            {
                "insert into Users([Name]) values('a name')".ExecuteNonQuery();

                "select top 1 Id from Users".ExecuteScalar().should_be(1);
            };

            it["doesn't allow the insert of id collumn"] = 
                expect<SqlException>(() => "insert into Users(Id) values(1)".ExecuteNonQuery());
        }

        void foreign_key_column()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "int", Identity = true },
                    new { Name = "nvarchar(255)" },
                    new { CustomerId = "int", ForeignKey = "Customers(Id)" }
                };
            
            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL IDENTITY(1,1),
                        [Name] nvarchar(255) NULL,
                        [CustomerId] int NULL FOREIGN KEY REFERENCES Customers(Id)
                    )
                ");

            it["the foreign key is nullable"] = () =>
            {
                "insert into Users([Name], [CustomerId]) values('a name', null)".ExecuteNonQuery();

                "select top 1 customerid from users".ExecuteScalar().should_be(DBNull.Value);
            };

            it["the foreign key constraint is adhered to"] = 
                expect<SqlException>(() => "insert into Users([Name], [CustomerId]) values('a name', 600)".ExecuteNonQuery());


            it["allows insert if foreign key matches"] = () =>
            {
                "insert into Customers([Name]) values('a name')".ExecuteNonQuery();

                "insert into Users([Name], [CustomerId]) values('a name', 1)".ExecuteNonQuery();
            };
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
                        [CustomerId] int NOT NULL FOREIGN KEY REFERENCES Customers(Id)
                    )
                ");

            it["the foreign key column does not accept nulls"] = 
                expect<SqlException>(() => "insert into Users([Name], [CustomerId]) values('a name', null)".ExecuteNonQuery());
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

        void generating_composite_primary_keys()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "int", PrimaryKey = true },
                    new { CustomerId = "int", PrimaryKey = true },
                    new { Name = "nvarchar(255)" }
                };

            it["contains primary key constraint for both columns"] = () =>
            {
                CommandShouldBe(@"
                    CREATE TABLE [dbo].[Users]
                    (
                        [Id] int NOT NULL,
                        [CustomerId] int NOT NULL,
                        [Name] nvarchar(255) NULL, 
                        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
                        (
                            [Id] ASC,
                            [CustomerId] ASC
                        )
                    )
                ");
            };

            it["multi column primary key constraint is adhered to"] = () =>
            {
                try
                {
                    "insert into Users(Id, CustomerId) values(1,1)".ExecuteScalar();

                    "insert into Users(Id, CustomerId) values(1,1)".ExecuteScalar();

                    throw new InvalidOperationException("SqlException not thrown");
                }
                catch (SqlException) { }
            };
        }
    }
}
