using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_has_one : nspec
    {
        object car1Id, car2Id, bluePrint1Id, bluePrint2Id;

        dynamic cars;

        Seed seed;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();

            cars = new Cars();

            seed.CreateTable("Cars",
                seed.Id(),
                new { Model = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("BluePrints",
                seed.Id(),
                new { CarId = "int" },
                new { Sku = "nvarchar(255)" }).ExecuteNonQuery();

            car1Id = new { Model = "car 1" }.InsertInto("Cars");

            car2Id = new { Model = "car 2" }.InsertInto("Cars");

            bluePrint1Id = new { CarId = car1Id, Sku = "Sku 1" }.InsertInto("BluePrints");
            
            bluePrint2Id = new { CarId = car2Id, Sku = "Sku 2" }.InsertInto("BluePrints");
        }

        void it_eager_loads_and_caches()
        {
            List<string> sqlQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = new Action<object, string, object[]>(
                (sender, sql, @params) =>
                {
                    sqlQueries.Add(sql);
                });

            var allCars = cars.All();

            var allBluePrints = allCars.BluePrint();

            var firstBluePrint = allCars.First().BluePrint();

            var lastBluePrint = allCars.Last().BluePrint();

            sqlQueries.Count.should_be(2);

            sqlQueries.First().should_contain("SELECT * FROM Cars");

            sqlQueries.Last().should_contain("in ('1','2')");
        }
    }
}
