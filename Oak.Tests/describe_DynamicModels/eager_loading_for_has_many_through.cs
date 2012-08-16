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
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("SupplyChains",
                seed.Id(),
                new { MarketId = "int" },
                new { SupplierId = "int" }).ExecuteNonQuery();

            seed.CreateTable("Suppliers",
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            supplier1Id = new { Name = "Supplier 1" }.InsertInto("Suppliers");

            supplier2Id = new { Name = "Supplier 2" }.InsertInto("Suppliers");

            market1Id = new { Name = "Market 1" }.InsertInto("Markets");

            market2Id = new { Name = "Market 2" }.InsertInto("Markets");

            new { MarketId = market1Id, SupplierId = supplier1Id }.InsertInto("SupplyChains");

            new { MarketId = market2Id, SupplierId = supplier1Id }.InsertInto("SupplyChains");
        }

        void it_eager_loads_child_collections_and_caches_them()
        {
            dynamic allMarkets = markets.All().Include("Suppliers");

            new { MarketId = market1Id, SupplierId = supplier2Id }.InsertInto("SupplyChains");

            new { MarketId = market2Id, SupplierId = supplier2Id }.InsertInto("SupplyChains");

            ((int)allMarkets.First().Suppliers().Count()).should_be(1);

            ((int)allMarkets.Last().Suppliers().Count()).should_be(1);
        }
    }
}
