using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.Classes;

namespace Oak.Tests.describe_DynamicModel
{
    class saving_dynamic_model_for_static_type : saving_dynamic_model
    {
        public override dynamic NewInventoryItem(object o)
        {
            return new InventoryItemWithAutoProps(o);
        }
    }

    class saving_dynamic_model_for_dynamic_type : saving_dynamic_model
    {
        public override dynamic NewInventoryItem(object o)
        {
            return new InventoryItem(o);
        }
    }
    
    abstract class saving_dynamic_model : nspec
    {
        public Seed seed;

        public dynamic item;

        public Inventory inventory;

        public dynamic itemId;

        void before_each()
        {
            inventory = new Inventory();

            seed = new Seed();

            seed.PurgeDb();

            seed.CreateTable("Inventory", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Sku = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        public abstract dynamic NewInventoryItem(object o);

        void saving_model()
        {
            context["new model"] = () =>
            {
                before = () => item = NewInventoryItem(new { Sku = "1112212" });

                act = () => item.Id = inventory.Insert(item);

                it["saves item"] = () =>
                {
                    var savedItem = inventory.Single(item.Id);

                    (savedItem.Sku as string).should_be("1112212");
                };
            };

            context["setting a property to null on a model that exists"] = () =>
            {
                before = () => 
                {
                    itemId = new { Sku = "11122212" }.InsertInto("Inventory");

                    item = inventory.Single(itemId);

                    (item.Sku as string).should_be("11122212");

                    item.Sku = null;

                    inventory.Save(item);
                };

                it["retains null value"] = () => (inventory.Single(itemId).Sku as string).should_be(null);
            };
        }
    }
}
