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

        dynamic authors;

        dynamic profiles;

        dynamic profileId;

        dynamic authorId;

        dynamic author;

        void before_each()
        {
            seed = new Seed();
        }

        void describe_has_one()
        {
            context["given author with profile"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    authors = new Authors();

                    CreateConventionalAuthorsTable();

                    CreateConventionalProfilesTable();

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

        void describe_unconventional_schema()
        {
            context["given foreign key does not match convention"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    authors = new UnconventionalAuthors();

                    CreateConventionalAuthorsTable();

                    seed.CreateTable("Profiles", new dynamic[] 
                    {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { fkAuthorId = "int", ForeignKey = "Authors(Id)" },
                        new { Email = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    authorId = new { Name = "Justin" }.InsertInto("Authors");

                    profileId = new { Email = "test@test.com", fkAuthorId = authorId }.InsertInto("Profiles");
                };

                context["retrieving author"] = () =>
                {
                    act = () => author = authors.Single(authorId);
                    
                    it["author should have profile"] = () => (author.Profile().Email as string).should_be("test@test.com");
                };
            };
        }

        void describe_cacheing()
        {
            context["given has one relation, has been accessed already"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    authors = new Authors();

                    profiles = new Profiles();

                    CreateConventionalAuthorsTable();

                    CreateConventionalProfilesTable();

                    authorId = new { Name = "Amir" }.InsertInto("Authors");

                    profileId = new { Email = "user@example.com", AuthorId = authorId }.InsertInto("Profiles");
                };

                act = () =>
                {
                    author = authors.Single(authorId);

                    author.Profile();
                };

                context["has one relation is changed from an external source"] = () =>
                {
                    act = () => profiles.Update(new { Email = "user2@example.com" }, profileId);

                    it["the reference via has one relation remains unchanged"] = () =>
                        (author.Profile().Email as string).should_be("user@example.com");
                };
            };
        }

        void CreateConventionalAuthorsTable()
        {
            seed.CreateTable("Authors", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        void CreateConventionalProfilesTable()
        {
            seed.CreateTable("Profiles", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { AuthorId = "int", ForeignKey = "Authors(Id)" },
                new { Email = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }
    }
}
