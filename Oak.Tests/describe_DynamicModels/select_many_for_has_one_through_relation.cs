using System;
using System.Linq;
using Oak.Tests.describe_DynamicModels.Classes;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many_for_has_one_through_relation : _dynamic_models
    {
        Stores stores;

        object storeId;

        object storeId2;

        object warehouseId;

        object warehouseId2;

        void before_each()
        {
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
        }

        void describe_select_many_off_of_collection()
        {
            before = () =>
            {
                storeId = new { Name = "Store 1" }.InsertInto("Stores");

                storeId2 = new { Name = "Store 2" }.InsertInto("Stores");

                warehouseId = new { Location = "LOC112" }.InsertInto("Warehouses");

                warehouseId2 = new { Location = "LOC333" }.InsertInto("Warehouses");

                new { StoreId = storeId, WarehouseId = warehouseId }.InsertInto("DistributionChannels");

                new { StoreId = storeId2, WarehouseId = warehouseId2 }.InsertInto("DistributionChannels");
            };

            act = () => models = stores.All();

            it["returns associated entries as collection"] = () =>
            {
                var warehouses = models.Warehouses();

                (warehouses.First().Location as string).should_be("LOC112");

                (warehouses.Last().Location as string).should_be("LOC333");

                (warehouses.First().Store.Name as string).should_be("Store 1");
            };
        }
    }
}
