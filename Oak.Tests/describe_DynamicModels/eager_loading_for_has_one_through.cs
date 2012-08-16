using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    [Tag("wip")]
    class eager_loading_for_has_one_through : nspec
    {
        Seed seed;

        dynamic stores;

        object storeId;

        object storeId2;

        object warehouseId;

        object warehouseId2;

        void before_each()
        {
            seed = new Seed();

            stores = new Stores();

            seed.PurgeDb();

            seed.CreateTable("Stores", new dynamic[] {
		                new { Id = "int", Identity = true, PrimaryKey = true },
				        new { Name = "nvarchar(255)" },
		            }).ExecuteNonQuery();

            seed.CreateTable("DistributionChannels", new dynamic[] {
		                new { Id = "int", Identity = true, PrimaryKey = true },
		                new { StoreId = "int" },
		                new { WarehouseId = "int" }
		            }).ExecuteNonQuery();

            seed.CreateTable("Warehouses", new dynamic[] {
		                new { Id = "int", Identity = true, PrimaryKey = true },
				        new { Location = "nvarchar(max)" },
		            }).ExecuteNonQuery();

            storeId = new { Name = "Store 1" }.InsertInto("Stores");

            storeId2 = new { Name = "Store 2" }.InsertInto("Stores");

            warehouseId = new { Location = "LOC112" }.InsertInto("Warehouses");

            warehouseId2 = new { Location = "LOC333" }.InsertInto("Warehouses");

            new { StoreId = storeId, WarehouseId = warehouseId }.InsertInto("DistributionChannels");

            new { StoreId = storeId2, WarehouseId = warehouseId2 }.InsertInto("DistributionChannels");
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

            var allStores = stores.All();

            var allWarehosues = allStores.Warehouse();

            allStores.First().Warehouse();

            allStores.Last().Warehouse();

            sqlQueries.Count.should_be(2);
        }
    }
}
