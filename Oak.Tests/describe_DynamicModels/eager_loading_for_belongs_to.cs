using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_belongs_to : nspec
    {
        object car1Id, car2Id, bluePrint1Id, bluePrint2Id, bluePrint3Id;

        dynamic bluePrints;

        Seed seed;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();

            bluePrints = new BluePrints();

            seed.CreateTable("Cars",
                new { Id = "int" },
                new { Model = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("BluePrints",
                new { Id = "int" },
                new { CarId = "int" },
                new { Sku = "nvarchar(255)" }).ExecuteNonQuery();

            car1Id = 100;

            new { Id = car1Id, Model = "car 1" }.InsertInto("Cars");

            car2Id = 200;

            new { Id = car2Id, Model = "car 2" }.InsertInto("Cars");

            bluePrint1Id = 300;
            
            new { Id = bluePrint1Id, CarId = car1Id, Sku = "Sku 1" }.InsertInto("BluePrints");
            
            bluePrint2Id = 400;

            new { Id = bluePrint2Id, CarId = car2Id, Sku = "Sku 2" }.InsertInto("BluePrints");

            bluePrint3Id = 500;

            new { Id = bluePrint3Id, CarId = car2Id, Sku = "Sku 3" }.InsertInto("BluePrints");
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

            var allBluePrints = bluePrints.All();

            var allCars = allBluePrints.Car();

            ((string)allBluePrints.First().Sku).should_be("Sku 1");

            ((string)allBluePrints.First().Car().Model).should_be("car 1");

            ((string)allBluePrints.Second().Sku).should_be("Sku 2");

            ((string)allBluePrints.Second().Car().Model).should_be("car 2");

            ((string)allBluePrints.Last().Sku).should_be("Sku 3");

            ((string)allBluePrints.Last().Car().Model).should_be("car 2");

            sqlQueries.Count.should_be(2);

            sqlQueries.First().should_contain("SELECT * FROM BluePrints");

            sqlQueries.Last().should_contain("in ('100','200')");
        }
    }
}
