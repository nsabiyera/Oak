using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class belongs_to : nspec
    {
        Comments comments;

        Seed seed;

        dynamic blogId;

        dynamic commentId;

        dynamic comment;

        void before_each()
        {
            seed = new Seed();

            comments = new Comments();
        }

        void describe_belongs_to()
        {
            context["given blogs that have many comments"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Blogs", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Title = "nvarchar(255)" },
                        new { Body = "nvarchar(max)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Comments", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                        new { Text = "nvarchar(1000)" }
                    }).ExecuteNonQuery();

                    blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                    commentId = new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
                };

                context["retrieving a blog associated with a comment"] = () =>
                {
                    act = () => comment = comments.Single(commentId);

                    it["returns blog associated with comment"] = () =>
                    {
                        (comment.Blog().Id as object).should_be(blogId as object);
                    };
                };
            };
        }
    }
}
