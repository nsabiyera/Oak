using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.belongs_to
{
    class belongs_to_unconventional_schema : nspec
    {
        Seed seed;

        dynamic blogId, commentId, comment, comments;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();
        }

        void describe_unconventional_schema()
        {
            context["given foreign key does not match convention"] = () =>
            {
                before = () =>
                {
                    comments = new UnconventionalComments();

                    CreateConventionalBlogTable();

                    seed.CreateTable("Comments", new dynamic[] {
		                        new { Id = "int", Identity = true, PrimaryKey = true },
		                        new { fkBlogId = "int", ForeignKey = "Blogs(Id)" },
		                        new { Text = "nvarchar(1000)" }
		                    }).ExecuteNonQuery();

                    blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                    commentId = new { fkBlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
                };

                VerifyBelongsToRetrieval();
            };

            context["given primary key does not match convention"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    comments = new UnconventionalComments2();

                    seed.CreateTable("Blogs", new dynamic[] {
		                        new { BlogId = "int", Identity = true, PrimaryKey = true },
		                        new { Title = "nvarchar(255)" },
		                        new { Body = "nvarchar(max)" }
		                    }).ExecuteNonQuery();

                    seed.CreateTable("Comments", new dynamic[] {
		                        new { Id = "int", Identity = true, PrimaryKey = true },
		                        new { BlogId = "int", ForeignKey = "Blogs(BlogId)" },
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
                        (comment.Blog().BlogId as object).should_be(blogId as object);
                    };
                };
            };
        }

        void VerifyBelongsToRetrieval()
        {
            context["retrieving a blog associated with a comment"] = () =>
            {
                act = () => comment = comments.Single(commentId);

                it["returns blog associated with comment"] = () =>
                {
                    (comment.Blog().Id as object).should_be(blogId as object);
                };
            };
        }

        void CreateConventionalBlogTable()
        {
            seed.CreateTable("Blogs", new dynamic[] {
		                new { Id = "int", Identity = true, PrimaryKey = true },
		                new { Title = "nvarchar(255)" },
		                new { Body = "nvarchar(max)" }
		            }).ExecuteNonQuery();
        }

        void CreateConventionalCommentTable()
        {
            seed.CreateTable("Comments", new dynamic[] {
		                new { Id = "int", Identity = true, PrimaryKey = true },
		                new { BlogId = "int", ForeignKey = "Blogs(Id)" },
		                new { Text = "nvarchar(1000)" }
		            }).ExecuteNonQuery();
        }
    }
}
