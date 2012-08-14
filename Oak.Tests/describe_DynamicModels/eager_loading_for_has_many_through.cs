using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    public class Stores : DynamicRepository
    {
        public Stores()
        {
            Projection = d => new Store(d);
        }   
    }

    public class Store : DynamicModel
    {
        Suppliers suppliers = new Suppliers();

        SupplyChains supplyChains = new SupplyChains();

        public Store(object dto)
            : base(dto)
        {
            
        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(suppliers, supplyChains);
        }
    }

    public class Suppliers : DynamicRepository
    {
        
    }

    public class SupplyChains : DynamicRepository
    {
        
    }

    [Tag("wip")]
    class eager_loading_for_has_many_through : nspec
    {
        Seed seed;

        dynamic stores;

        object store1Id, store2Id, supplier1Id, supplier2Id;

        void before_each()
        {
            seed = new Seed();

            stores = new Stores();

            seed.PurgeDb();

            seed.CreateTable("Stores",
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("SupplyChains",
                seed.Id(),
                new { StoreId = "int" },
                new { SupplierId = "int" }).ExecuteNonQuery();

            seed.CreateTable("Suppliers",
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            supplier1Id = new { Name = "Supplier 1" }.InsertInto("Suppliers");

            supplier2Id = new { Name = "Supplier 2" }.InsertInto("Suppliers");

            store1Id = new { Name = "Store 1" }.InsertInto("Stores");

            store2Id = new { Name = "Store 2" }.InsertInto("Stores");

            new { StoreId = store1Id, SupplierId = supplier1Id }.InsertInto("SupplyChains");

            new { StoreId = store2Id, SupplierId = supplier1Id }.InsertInto("SupplyChains");
        }

        void it_eager_loads_child_collections_and_caches_them()
        {
            dynamic allStores = stores.All().Include("Suppliers");

            new { StoreId = store1Id, SupplierId = supplier2Id }.InsertInto("SupplyChains");

            new { StoreId = store2Id, SupplierId = supplier2Id }.InsertInto("SupplyChains");

            ((int)allStores.First().Suppliers().Count()).should_be(1);

            ((int)allStores.Last().Suppliers().Count()).should_be(1);
        }
    }
}
