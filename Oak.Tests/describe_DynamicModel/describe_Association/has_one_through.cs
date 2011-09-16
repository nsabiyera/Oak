using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class has_one_through : nspec
    {
        Seed seed;

        dynamic customerId;

        dynamic supplierId;

        Customers customers;

        void before_each()
        {
            seed = new Seed();

            customers = new Customers();
        }

        void describe_has_one_through()
        {
            context["a store has one supplier through distribution channels"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Customers", new dynamic[] { 
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Name = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Suppliers", new dynamic[] { 
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Name = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("DistributionChannels", new dynamic[] { 
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { CustomerId = "int", ForeignKey = "Customers(Id)" },
                        new { SupplierId = "int", ForeignKey = "Suppliers(Id)" }
                    }).ExecuteNonQuery();

                    customerId = new { Name = "Apple Store" }.InsertInto("Customers");

                    supplierId = new { Name = "Texas Instruments" }.InsertInto("Suppliers");

                    new { CustomerId = customerId, SupplierId = supplierId }.InsertInto("DistributionChannels");
                };

                it["retrieves has one through"] = () => 
                    (customers.Single(customerId).Supplier().Name as string).should_be("Texas Instruments");
                    
            };
        }
    }
}
