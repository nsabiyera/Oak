using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_Seed
{
    class when_adding_columns : _seed
    {
        void act_each()
        {
            command = seed.AddColumns("Users", columns);
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
                        ALTER TABLE [dbo].[Users] ADD [FooBar] int NULL
                    ");
        }

        void add_not_null_int_column()
        {
            before = () =>
                    columns = new[] 
                    {
                        new { FooBar = "int", Nullable = false }
                    };

            it["creates the alter table statement"] = () =>
                CommandShouldBe(@"
                        ALTER TABLE [dbo].[Users] ADD [FooBar] int NOT NULL
                    ");
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
                    ALTER TABLE [dbo].[Users] ADD [Column1] int NULL, [Column2] int NULL
                ");
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
                    ALTER TABLE [dbo].[Users] ADD [FooBar] int NOT NULL DEFAULT('10')
                ");
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
                    ALTER TABLE [dbo].[Users] ADD [Column1] int NOT NULL DEFAULT('10'), [Column2] nvarchar(255) NULL DEFAULT('Test')
                ");
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
                    ALTER TABLE [dbo].[Users] ADD [Column1] datetime NOT NULL DEFAULT(getdate())
                ");
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
                    ALTER TABLE [dbo].[Users] ADD [Column1] uniqueidentifier NOT NULL DEFAULT(newid())
                ");
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
                    ALTER TABLE [dbo].[Users] ADD [Column1] int FOREIGN KEY REFERENCES Customers(Id)
                ");
        }
    }
}
