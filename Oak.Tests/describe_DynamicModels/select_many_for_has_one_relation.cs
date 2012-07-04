using System;
using System.Linq;
using Oak.Tests.describe_DynamicModels.Classes;
using NSpec;
using Massive;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many_for_has_one_relation : _dynamic_models
    {
        Users users;

        object userId;

        object userId2;

        void before_each()
        {
            users = new Users();

            seed.PurgeDb();

            seed.CreateTable("Users", new dynamic[] 
		    {
		        new { Id = "int", Identity = true, PrimaryKey = true },
		        new { Name = "nvarchar(255)" },
		    }).ExecuteNonQuery();

            seed.CreateTable("Profiles", new dynamic[] 
		    {
		        new { Id = "int", Identity = true, PrimaryKey = true },
		        new { UserId = "int" },
		        new { DisplayName = "nvarchar(255)" }
		    }).ExecuteNonQuery();
        }

        void select_many_off_of_collection()
        {
            before = () =>
            {
                userId = new { Name = "Jane Doe" }.InsertInto("Users");

                userId2 = new { Name = "John Doe" }.InsertInto("Users");

                new { DisplayName = "Jane", UserId = userId }.InsertInto("Profiles");

                new { DisplayName = "John", UserId = userId2 }.InsertInto("Profiles");
            };

            act = () => models = users.All();

            it["returns associated entities as collection"] = () =>
            {
                var users = models.Profiles();

                (users.First().DisplayName as string).should_be("Jane");

                (users.Last().DisplayName as string).should_be("John");

                (users.First().User.Name as string).should_be("Jane Doe");
            };
        }
    }
}
