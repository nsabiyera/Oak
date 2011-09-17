using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.Classes;

namespace Oak.Tests.describe_DynamicModel
{
    class saving_dynamic_model : nspec
    {
        Seed seed;

        dynamic item;

        Inventory inventory;

        void before_each()
        {
            inventory = new Inventory();

            seed = new Seed();

            seed.CreateTable("Inventory", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Sku = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        void saving_model()
        {
            act = () => item.Id = inventory.Save(item);

            context["new model"] = () =>
            {
                before = () => item = new InventoryItem(new { Sku = "1112212" });

                it["saves item"] = () =>
                {
                    var savedItem = inventory.Single(item.Id);

                    (savedItem.Sku as string).should_be("1112212");
                };
            };
        }
    }
}
