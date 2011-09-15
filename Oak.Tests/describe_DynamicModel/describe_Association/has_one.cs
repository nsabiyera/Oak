using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class has_one : nspec
    {
        Seed seed;

        Authors authors;

        dynamic profileId;

        dynamic authorId;

        dynamic author;

        void before_each()
        {
            seed = new Seed();

            authors = new Authors();
        }

        void describe_has_one()
        {
            context["given author with profile"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Authors", new dynamic[] 
                    {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Name = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Profiles", new dynamic[] 
                    {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { AuthorId = "int", ForeignKey = "Authors(Id)" },
                        new { Email = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    authorId = new { Name = "Amir" }.InsertInto("Authors");

                    profileId = new { Email = "user@example.com", AuthorId = authorId }.InsertInto("Profiles");
                };

                context["retrieving author"] = () =>
                {
                    act = () => author = authors.Single(authorId);

                    it["author should have profile"] = () => (author.Profile().Email as string).should_be("user@example.com");
                };
            };
        }
    }
}
