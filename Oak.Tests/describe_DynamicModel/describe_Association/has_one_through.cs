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
       dynamic customer;

        Seed seed;

        dynamic customerId;

        dynamic supplierId;

        Customers customers;

        Suppliers suppliers;

        void before_each()
        {
            seed = new Seed();

            customers = new Customers();

            suppliers = new Suppliers();
        }

        void describe_has_one_through()
        {
            context["a store has one supplier through distribution channels"] = () =>
            {
                before = () =>
                {
                    CreateCustomerAndSupplierTables();

                    customerId = new { Name = "Apple Store" }.InsertInto("Customers");

                    supplierId = new { Name = "Texas Instruments" }.InsertInto("Suppliers");

                    new { CustomerId = customerId, SupplierId = supplierId }.InsertInto("DistributionChannels");
                };

                it["retrieves has one through"] = () =>
                {
                    var customer = customers.Single(customerId);

                    (customer.suppliers as object).should_not_be_null();

                    (customer.Supplier().Name as string).should_be("Texas Instruments");
                };
            };
        }

        void describe_cacheing()
        {
            before = () =>
            {
                CreateCustomerAndSupplierTables();

                customerId = new { Name = "Apple Store" }.InsertInto("Customers");

                supplierId = new { Name = "Texas Instruments" }.InsertInto("Suppliers");

                new { CustomerId = customerId, SupplierId = supplierId }.InsertInto("DistributionChannels");
            };

            context["has one through has been accessed"] = () =>
            {
                act = () =>
                {
                    customer = customers.Single(customerId);

                    customer.Supplier();
                };

                context["relation is altered through an external source"] = () =>
                {
                    act = () => suppliers.Update(new { Name = "Texas Instruments Changed" }, supplierId);

                    it["has one reference is unchanged"] = () =>
                        (customer.Supplier().Name as string).should_be("Texas Instruments");

                    it["discarding cache reflects changes"] = () =>
                        (customer.Supplier(discardCache: true).Name as string).should_be("Texas Instruments Changed");
                };
            };
        }

        void CreateCustomerAndSupplierTables()
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
        }
    }
}
