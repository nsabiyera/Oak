using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class has_many : nspec
    {
        Seed seed;

        dynamic blogId;

        dynamic otherBlogId;

        IEnumerable<dynamic> blogComments;

        Blogs blogs;

        dynamic blog;

        dynamic comment;

        Comments comments;

        void before_each()
        {
            seed = new Seed();

            blogs = new Blogs();

            comments = new Comments();
        }

        void describe_has_many()
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

                    new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");

                    new { BlogId = blogId, Text = "Comment 2" }.InsertInto("Comments");

                    otherBlogId = new { Title = "Other Blog", Body = "Other Body" }.InsertInto("Blogs");

                    new { BlogId = otherBlogId, Text = "Comment 3" }.InsertInto("Comments");
                };

                context["retrieving comments for blog"] = () =>
                {
                    act = () => blogComments = blogs.Single(blogId).Comments();

                    it["has two comments"] = () =>
                    {
                        blogComments.Count().should_be(2);

                        blogComments.should_contain(s => s.Text == "Comment 1");

                        blogComments.should_contain(s => s.Text == "Comment 2");
                    };
                };

                context["retrieving comments for other blog"] = () =>
                {
                    act = () => blogComments = blogs.Single(otherBlogId).Comments();
                    
                    it["has one comments"] = () =>
                    {
                        blogComments.Count().should_be(1);

                        blogComments.should_contain(s => s.Text == "Comment 3");
                    };
                };
            };
        }

        void newing_up_has_many_association_for_blog()
        {
            before = () =>
            {
                blog = new Blog();
                blog.Id = 100;
            };

            context["building a comment for a blog"] = () =>
            {
                act = () => comment = blog.Comments().New();

                it["sets the blog id for comment"] = () => ((int)comment.BlogId).should_be(blog.Id as object);

                it["is of type defined in projection"] = () => (comment as object).should_cast_to<Comment>();
            };

            context["building a comment for a blog with attributes"] = () =>
            {
                act = () => comment = blog.Comments().New(new { Text = "Hi" });

                it["sets additional attributes"] = () => ((string)comment.Text).should_be("Hi");

                it["is of type defined in projection"] = () => (comment as object).should_cast_to<Comment>();
            };

            context["building a comment where the blog id is specified"] = () =>
            {
                act = () => comment = blog.Comments().New(new { BlogId = 20 });

                it["overrides the id"] = () => ((int)comment.BlogId).should_be(blog.Id as object);
            };
        }

        void saving_association_that_has_been_newed_up_through_parent_entity()
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
            };

            act = () =>
            {
                blog = blogs.Single(blogId);

                comment = blog.Comments().New(new { Text = "hello" });

                comments.Save(comment);
            };

            it["blog should have saved comments"] = () => ((blog.Comments() as IEnumerable<dynamic>).First().Text as string).should_be("hello");
        }
    }
}
