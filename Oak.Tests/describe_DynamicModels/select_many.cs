using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many : _dynamic_models
    {
        dynamic user1Id;

        void selecting_many_off_of_collection()
        {
            before = () =>
            {
                seed.CreateTable("Users", new dynamic[] 
                { 
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.CreateTable("Emails", new dynamic[] 
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { UserId = "int" },
                    new { Address = "nvarchar(255)" }
                }).ExecuteNonQuery();

                user1Id = new { Name = "Jane" }.InsertInto("Users");
            };
        }
    }
}
