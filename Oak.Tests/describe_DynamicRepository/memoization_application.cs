using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicRepository
{
    class memoization_application : nspec
    {
        Seed seed;

        dynamic records;

        void before_each()
        {
            seed = new Seed();

            records = new MemoizedRecords();

            seed.PurgeDb();
        }

        void specify_memoization_can_be_applied_to_dynamic_repository()
        {
            var executedSql = new List<string>();

            seed.CreateTable("Records", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            new { Name = "A Name" }.InsertInto("Records");

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = (sender, sql, args) => executedSql.Add(sql);

            dynamic allRows = (records.CurrentRecords("A") as IEnumerable<dynamic>).ToList();

            allRows = (records.CurrentRecords("A") as IEnumerable<dynamic>).ToList();

            (allRows.Count as object).should_be(1);

            executedSql.Count.should_be(1);
        }
    }
}
