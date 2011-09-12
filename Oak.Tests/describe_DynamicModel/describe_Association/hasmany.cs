using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak;
using Oak.Tests.SampleClasses;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class hasmany : nspec
    {
        Seed seed;

        dynamic blogId;

        dynamic otherBlogId;

        IEnumerable<dynamic> comments;

        Blogs blogs;

        void before_each()
        {
            seed = new Seed();

            blogs = new Blogs();
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

                    otherBlogId = new { Title = "Other Blog", Body = "Other Body" }.InsertInto("Blogs");

                    new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");

                    new { BlogId = blogId, Text = "Comment 2" }.InsertInto("Comments");

                    new { BlogId = otherBlogId, Text = "Comment 3" }.InsertInto("Comments");
                };

                context["retrieving comments for blog"] = () =>
                {
                    act = () => comments = blogs.Single(blogId).Comments();

                    it["has two comments"] = () =>
                    {
                        comments.Count().should_be(2);

                        comments.should_contain(s => s.Text == "Comment 1");

                        comments.should_contain(s => s.Text == "Comment 2");
                    };
                };

                context["retrieving comments for other blog"] = () =>
                {
                    act = () => comments = blogs.Single(otherBlogId).Comments();
                    
                    it["has one comments"] = () =>
                    {
                        comments.Count().should_be(1);

                        comments.should_contain(s => s.Text == "Comment 3");
                    };
                };
            };
        }

        void describe_has_many_through()
        {
            
        }
    }
}
