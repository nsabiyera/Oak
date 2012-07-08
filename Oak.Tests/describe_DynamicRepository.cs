using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests
{
    public class Records : DynamicRepository { }

    class describe_DynamicRepository : nspec
    {
        Seed seed;

        Records records;

        object recordToInsert;

        void before_each()
        {
            seed = new Seed();

            records = new Records();

            seed.PurgeDb();
        }

        void inserting_a_record_that_has_non_value_type_properties()
        {
            before = () =>
            {
                seed.CreateTable("Records", new dynamic[] 
                { 
                   seed.Id(),
                   new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                recordToInsert = new { Name = "foo", NotAValueType = new { Name = "bar" } };
            };

            act = () => records.Insert(recordToInsert);

            it["the record is inserted (ignoring the reference type)"] = () =>
            {
                var record = records.All().First();

                record.Name_is("foo");
            };
        }

        [Tag("wip")]
        void updating_every_type_of_sql_column()
        {
            before = () =>
            {
                var sql = seed.CreateTable("Records", new dynamic[] 
                { 
                   new { BigIntColumn = "bigint" },
                   //new { BinaryColumn = "binary(50)" },
                   new { BitColumn = "bit" },
                   //new { BinaryColumn = "char(10)" },
                   new { DateColumn = "date" },
                   new { DateTimeColumn = "datetime" },
                   new { DateTimeTwoColumn = "datetime2(7)" },
                   new { DateTimeOffSetColumn = "datetimeoffset(7)" }
                });

                Console.WriteLine(sql);

                sql.ExecuteNonQuery();

                recordToInsert = new
                {
                    BigIntColumn = 10,
                    BitColumn = true,
                    DateColumn = DateTime.Today,
                    DateTimeColumn = DateTime.Today.AddDays(1).AddHours(1),
                    DateTimeTwoColumn = DateTime.Today.AddDays(2).AddHours(2),
                    DateTimeOffSetColumn = new DateTimeOffset(DateTime.Today.AddDays(3).AddHours(3))
                };
            };

            act = () => records.Insert(recordToInsert);

            it["each column is updated"] = () =>
            {
                var record = records.All().First();

                Console.WriteLine(record);

                ((long)record.BigIntColumn).should_be(10);

                ((bool)record.BitColumn).should_be(true);

                ((DateTime)record.DateColumn).should_be(DateTime.Today);

                ((DateTime)record.DateTimeColumn).should_be(DateTime.Today.AddDays(1).AddHours(1));

                ((DateTime)record.DateTimeTwoColumn).should_be(DateTime.Today.AddDays(2).AddHours(2));

                ((DateTimeOffset)record.DateTimeOffSetColumn).should_be(new DateTimeOffset(DateTime.Today.AddDays(3).AddHours(3)));
            };
        }
    }
}
