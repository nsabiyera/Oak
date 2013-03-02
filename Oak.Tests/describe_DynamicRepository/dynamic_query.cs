using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;

namespace Oak.Tests.describe_DynamicRepository
{
    [Tag("wip")]
    class dynamic_query : nspec
    {
        dynamic records;

        Seed seed = new Seed();

        void before_each()
        {
            records = new Records();

            seed.PurgeDb();

            seed.CreateTable("Records", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" },
                new { Dob = "datetime" },
                new { Weight = "int" }
            }).ExecuteNonQuery();
        }

        void describe_find_by()
        {
            it["order by"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.FindBy(orderby: "Name");

                (result.First().Name as string).should_be("A Name");
            };

            it["columns"] = () =>
            {
                new { Dob = DateTime.Today }.InsertInto("Records");

                var result = records.FindBy(columns: "Dob");

                ((bool)result.First().RespondsTo("Name")).should_be(false);
            };

            it["equality specification"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.FindBy(name: "A Name");

                (result.First().Name as string).should_be("A Name");

                ((int)result.Count()).should_be(1);
            };

            it["all"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.FindBy();

                ((int)result.Count()).should_be(2);
            };
        }

        void describe_count()
        {
            it["without equality specification"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.Count();

                ((int)result).should_be(2);
            };

            it["with equality specification"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.Count(name: "A Name");

                ((int)result).should_be(1);
            };
        }

        void describe_sum()
        {
            it["without equality specification"] = () =>
            {
                new { Weight = 100 }.InsertInto("Records");

                new { Weight = 16 }.InsertInto("Records");

                var result = (int)records.Sum(columns: "Weight");

                result.should_be(116);
            };

            it["with equality specification"] = () =>
            {
                new { Name = "A Name", Weight = 100 }.InsertInto("Records");

                new { Name = "B Name",  Weight = 16 }.InsertInto("Records");

                var result = (int)records.Sum(columns: "Weight", Name: "A Name");

                result.should_be(100);
            };
        }

        void describe_max()
        {
            it["without equality specification"] = () =>
            {
                new { Weight = 100 }.InsertInto("Records");

                new { Weight = 16 }.InsertInto("Records");

                var result = (int)records.Max(columns: "Weight");

                result.should_be(100);
            };

            it["with equality specification"] = () =>
            {
                new { Name = "A Name", Weight = 100 }.InsertInto("Records");

                new { Name = "B Name", Weight = 16 }.InsertInto("Records");

                var result = (int)records.Max(columns: "Weight", Name: "B Name");

                result.should_be(16);
            };
        }

        void describe_min()
        {
            it["without equality specification"] = () =>
            {
                new { Weight = 100 }.InsertInto("Records");

                new { Weight = 16 }.InsertInto("Records");

                var result = (int)records.Min(columns: "Weight");

                result.should_be(16);
            };

            it["with equality specification"] = () =>
            {
                new { Name = "A Name", Weight = 100 }.InsertInto("Records");

                new { Name = "B Name", Weight = 16 }.InsertInto("Records");

                var result = (int)records.Min(columns: "Weight", Name: "A Name");

                result.should_be(100);
            };
        }

        void describe_avg()
        {
            it["without equality specification"] = () =>
            {
                new { Weight = 100 }.InsertInto("Records");

                new { Weight = 16 }.InsertInto("Records");

                var result = (int)records.Avg(columns: "Weight");

                result.should_be(58);
            };

            it["with equality specification"] = () =>
            {
                new { Name = "A Name", Weight = 100 }.InsertInto("Records");

                new { Name = "B Name", Weight = 16 }.InsertInto("Records");

                var result = (int)records.Avg(columns: "Weight", Name: "A Name");

                result.should_be(100);
            };
        }

        void describe_first_get_single()
        {
            it["order by"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.First(orderby: "Name desc");

                (result.Name as string).should_be("B Name");
            };

            it["columns"] = () =>
            {
                new { Dob = DateTime.Today }.InsertInto("Records");

                var result = records.Get(columns: "Dob");

                ((bool)result.RespondsTo("Name")).should_be(false);
            };

            it["equality specification"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.Select(name: "A Name");

                (result.Name as string).should_be("A Name");
            };
        }

        void describe_last()
        {
            it["order by"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.Last(orderby: "Name");

                (result.Name as string).should_be("B Name");
            };

            it["columns"] = () =>
            {
                new { Dob = DateTime.Today }.InsertInto("Records");

                var result = records.Last(columns: "Dob");

                ((bool)result.RespondsTo("Name")).should_be(false);
            };

            it["equality specification"] = () =>
            {
                new { Name = "B Name" }.InsertInto("Records");

                new { Name = "A Name" }.InsertInto("Records");

                var result = records.Last(name: "A Name");

                (result.Name as string).should_be("A Name");
            };
        }
    }
}
