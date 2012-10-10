using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;

namespace Oak.Tests.describe_DynamicRepository
{
    class pagination : nspec
    {
        Seed seed;

        Records records;

        void before_each()
        {
            seed = new Seed();

            records = new Records();

            seed.PurgeDb();

            seed.CreateTable("Records", new dynamic[] 
            { 
                new { Id = "int" },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.CreateTable("OtherRecords", new dynamic[] 
            { 
                new { Id = "int" },
                new { RecordId = "int" },
                new { Name2 = "nvarchar(255)" }
            }).ExecuteNonQuery();

            new string[] 
            {
                "Record 100", "Record 101",
                "Record 102", "Record 103",
                "Record 104", "Record 105",
                "Record 106", "Record 107",
                "Record 108", "Record 109",
                "Record 201", "Record 202", 
                "Record 203", "Record 204", 
                "Record 205", "Record 206", 
                "Record 207", "Record 208", 
                "Record 209", "Record 200"
            }.ForEach(s => new
            {
                Id = int.Parse(s.Replace("Record ", "")),
                Name = s
            }.InsertInto("Records"));

            new string[] 
            {
                "Record 30100", "Record 30101",
                "Record 30102", "Record 30103",
                "Record 30104", "Record 30105",
                "Record 30106", "Record 30107",
                "Record 30108", "Record 30109",
                "Record 40201", "Record 40202", 
                "Record 40203", "Record 40204", 
                "Record 40205", "Record 40206", 
                "Record 40207", "Record 40208", 
                "Record 40209", "Record 40200"
            }.ForEach(s => new
            {
                Id = int.Parse(s.Replace("Record ","")),
                RecordId = int.Parse(
                    s.Replace("Record ","")
                     .Replace("30", "")
                     .Replace("40","")),
                Name2 = s
            }.InsertInto("OtherRecords"));
        }

        void specify_paged_for_repo()
        {
            var result = records.Paged("Name like @0", pageSize: 5, args: "%10%").Items;

            ((int)result.Count()).should_be(5);
        }

        void specify_paged_query_for_repo()
        {
            var result = records.PagedQuery(
                @"select r.* 
                  from records r 
                  inner join otherrecords other 
                  on r.Id = other.RecordId 
                  where Name2 like @0", pageSize: 5, args: "%30%").Items;

            ((int)result.Count()).should_be(5);
        }
    }
}
