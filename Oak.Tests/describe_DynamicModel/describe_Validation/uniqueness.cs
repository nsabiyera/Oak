using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class uniqueness : nspec
    {
        Seed seed;

        Users users;

        dynamic user;

        void before_each()
        {
            seed = new Seed();

            users = new Users();
        }

        void describe_uniqueness()
        {
            before = () =>
            {
                seed.PurgeDb();

                seed.CreateTable("Users", new dynamic[] {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Email = "nvarchar(255)" }
                }).ExecuteNonQuery();

                new { Email = "user@example.com" }.InsertInto("Users");
            };

            context["email associated with users is not taken"] = () =>
            {
                before = () =>
                {
                    user = new User();
                    user.Email = "nottaken@example.com";
                };

                it["user is valid"] = () => ((bool)user.IsValid()).should_be_true();
            };

            context["email associated with users is taken"] = () =>
            {
                before = () =>
                {
                    user = new User();
                    user.Email = "user@example.com";
                };

                it["user is valid"] = () => ((bool)user.IsValid()).should_be_false();
            };
        }
    }
}
