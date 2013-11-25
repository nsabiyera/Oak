using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Data.SqlClient;
using System.Data.SqlServerCe;

namespace Oak.Tests.describe_Seed
{
    class when_adding_columns : _seed
    {
        void before_each()
        {
            seed.PurgeDb();

            seed.CreateTable("Users",
                seed.Id(),
                new { AnExistingColumn = "nvarchar(255)" }
            ).ExecuteNonQuery();

            seed.CreateTable("Customers",
                seed.Id(),
                new { AnExistingColumn = "nvarchar(255)" }
            ).ExecuteNonQuery();
        }

        void act_each()
        {
            command = seed.AddColumns("Users", columns);

            command.ExecuteNonQuery();
        }

        void add_int_column()
        {
            before = () =>
                columns = new[] 
                {
                    new { FooBar = "int" }
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                        ALTER TABLE [Users] ADD [FooBar] int NULL
                    ");

            it["the column can be inserted into"] = () => 
            {
                "insert into Users(FooBar) values(42)".ExecuteNonQuery();
                "select top 1 FooBar from Users".ExecuteScalar().should_be(42);
            };

            it["nulls are allowed"] = () =>
            {
                "insert into Users(FooBar) values(null)".ExecuteNonQuery();
                "select top 1 FooBar from Users".ExecuteScalar().should_be(DBNull.Value);
            };
        }

        void add_not_null_int_column()
        {
            before = () =>
                columns = new[] 
                {
                    new { FooBar = "int", Nullable = false }
                };
            if (IsSqlCe()) return;

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                        ALTER TABLE [Users] ADD [FooBar] int NOT NULL
                    ");

            it["nulls are not allowed"] = expect<SqlException>(() =>
                "insert into Users(FooBar) values(null)".ExecuteNonQuery());
        }

        void add_two_int_columns()
        {
            before = () =>
                columns = new dynamic[] 
                {
                    new { Column1 = "int" },
                    new { Column2 = "int" }
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [Column1] int NULL, [Column2] int NULL
                ");

            it["both columns can be inserted into"] = () =>
            {
                "insert into Users(Column1, Column2) values(42, 55)".ExecuteNonQuery();
                "select top 1 Column1 from Users".ExecuteScalar().should_be(42);
                "select top 1 Column2 from Users".ExecuteScalar().should_be(55);
            };

            it["nulls are allowed"] = () =>
            {
                "insert into Users(Column1, Column2) values(null, null)".ExecuteNonQuery();
                "select top 1 Column1 from Users".ExecuteScalar().should_be(DBNull.Value);
                "select top 1 Column2 from Users".ExecuteScalar().should_be(DBNull.Value);
            };
        }

        void column_with_default_value()
        {
            before = () =>
                columns = new[] 
                {
                    new { FooBar = "int", Default = 10, Nullable = false }
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [FooBar] int NOT NULL DEFAULT('10')
                ");

            it["default value is adhered to"] = () =>
            {
                "insert into Users(AnExistingColumn) values('Existing Value')".ExecuteNonQuery();

                "select top 1 FooBar from Users".ExecuteScalar().should_be(10);
            };
        }

        void two_columns_with_default_values()
        {
            before = () =>
                columns = new dynamic[] 
                {
                    new { Column1 = "int", Default = 10, Nullable = false },
                    new { Column2 = "nvarchar(255)", Default = "Test" }
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [Column1] int NOT NULL DEFAULT('10'), [Column2] nvarchar(255) NULL DEFAULT('Test')
                ");

            it["default values are adhered to"] = () =>
            {
                "insert into Users(AnExistingColumn) values('Existing Value')".ExecuteNonQuery();

                "select top 1 Column1 from Users".ExecuteScalar().should_be(10);

                "select top 1 Column2 from Users".ExecuteScalar().should_be("Test");
            };
        }

        void add_date_column_with_default_value_of_GETDATE()
        {
            before = () =>
                columns = new dynamic[] 
                {
                    new { Column1 = "datetime", Default = "getdate()", Nullable = false },
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [Column1] datetime NOT NULL DEFAULT(getdate())
                ");

            it["default value is set to todays date"] = () =>
            {
                "insert into Users(AnExistingColumn) values('Existing Value')".ExecuteNonQuery();

                "select top 1 Column1 from Users".ExecuteScalar().ToString().should_be(DateTime.Now.ToString());
            };
        }

        void add_guid_column_with_default_value_of_newid()
        {
            before = () =>
                columns = new dynamic[] 
                {
                    new { Column1 = "uniqueidentifier", Default = "newid()", Nullable = false },
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [Column1] uniqueidentifier NOT NULL DEFAULT(newid())
                ");

            it["generates a new guid"] = () =>
            {
                "insert into Users(AnExistingColumn) values('Existing Value')".ExecuteNonQuery();

                "select top 1 Column1 from Users".ExecuteScalar().should_not_be(Guid.Empty);

                "select top 1 Column1 from Users".ExecuteScalar().should_not_be(null);
            };
        }

        void add_foreign_key_column()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Column1 = "int", ForeignKey = "Customers(Id)" },
                };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                    ALTER TABLE [Users] ADD [Column1] int NULL REFERENCES Customers(Id)
                ");

            it["foreign key constraint is applied"] = expect<SqlCeException>(() =>
            {
                "insert into Users(Column1) values(42)".ExecuteNonQuery();
            });

            it["allows insert if record exists in constraint table"] = () =>
            {
                "insert into Customers (AnExistingColumn) values('A Value')".ExecuteNonQuery();

                "insert into Users(Column1) values(1)".ExecuteNonQuery();

                @"select top 1 AnExistingColumn 
                  from Customers 
                  where Id in 
                  (
                     select top 1 Column1 
                     from Users
                  )".ExecuteScalar().should_be("A Value");
            };
        }
    }
}
