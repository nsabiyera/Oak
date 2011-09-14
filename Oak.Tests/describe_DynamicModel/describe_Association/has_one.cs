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

        dynamic blogId;

        dynamic authorId;

        Blogs blogs;

        dynamic author;

        void before_each()
        {
            seed = new Seed();

            blogs = new Blogs();
        }

        void describe_has_one()
        {
            context["given blog with author"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Authors", new dynamic[] 
                    {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Name = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Blogs", new dynamic[] 
                    {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { AuthorId = "int", ForeignKey = "Authors(Id)" },
                        new { Title = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    authorId = new { Name = "Amir" }.InsertInto("Authors");

                    blogId = new { Title = "SomeTitle", AuthorId = authorId }.InsertInto("Blogs");
                };

                context["retrieving author for blog"] = () =>
                {
                    act = () => author = blogs.Single(blogId).Author();

                    it["author should be retrieved"] = () => (author.Name as string).should_be("Amir");
                };
            };
        }
    }
}
