using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_has_many_through : nspec
    {
        Seed seed;

        dynamic markets;

        object market1Id, market2Id, supplier1Id, supplier2Id;

        void before_each()
        {
            seed = new Seed();

            markets = new Markets();

            seed.PurgeDb();

            seed.CreateTable("Markets",
                new { Id = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("SupplyChains",
                new { Id = "int" },
                new { MarketId = "int" },
                new { SupplierId = "int" }).ExecuteNonQuery();

            seed.CreateTable("Suppliers",
                new { Id = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            supplier1Id = 100;
            new { Id = supplier1Id, Name = "Supplier 1" }.InsertInto("Suppliers");

            supplier2Id = 200;
            new { Id = supplier2Id, Name = "Supplier 2" }.InsertInto("Suppliers");

            market1Id = 300;
            new { Id = market1Id, Name = "Market 1" }.InsertInto("Markets");

            market2Id = 400;
            new { Id = market2Id, Name = "Market 2" }.InsertInto("Markets");

            new { Id = 500, MarketId = market1Id, SupplierId = supplier1Id }.InsertInto("SupplyChains");

            new { Id = 600, MarketId = market2Id, SupplierId = supplier2Id }.InsertInto("SupplyChains");
        }

        void it_eager_loads_child_collections_and_caches_them()
        {
            List<string> sqlQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = new Action<object, string, object[]>(
                (sender, sql, @params) =>
                {
                    sqlQueries.Add(sql);
                });

            dynamic allMarkets = markets.All().Include("Suppliers");

            ((int)allMarkets.First().Suppliers().Count()).should_be(1);

            (allMarkets.First().Suppliers().First().Name as string).should_be("Supplier 1");

            ((int)allMarkets.Last().Suppliers().Count()).should_be(1);

            (allMarkets.Last().Suppliers().First().Name as string).should_be("Supplier 2");

            sqlQueries.Count.should_be(2);
        }

        void specify_eager_loaded_collections_retain_creation_methods()
        {
            dynamic firstMarket = markets.All().Include("Suppliers", "SupplyChains").First();

            var supplierId = 1000;

            var supplier = firstMarket.Suppliers().New(new { Id = supplierId, Name = "Market 3" });

            new Suppliers().Insert(supplier);

            var supplyChain = firstMarket.SupplyChains().New(new { Id = 900, SupplierId = supplierId });

            new SupplyChains().Insert(supplyChain);

            firstMarket = markets.All().Include("Suppliers").First();

            ((int)firstMarket.Suppliers().Count()).should_be(2);
        }
    }
}
