using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using DynamicBlog.Models;
using Oak;
using Oak.Controllers;

namespace DynamicBlog.Tests.Models
{
    class describe_Blogs : nspec
    {
        Blogs blogs;

        SeedController seedController;

        IEnumerable<dynamic> retrievedBlogs;

        Guid blogId;

        void before_each()
        {
            blogs = new Blogs();

            seedController = new SeedController();

            seedController.PurgeDb();

            seedController.All();

            blogId = Guid.NewGuid();
        }

        void when_retrieving_blogs()
        {
            act = () => retrievedBlogs = blogs.All();

            context["given no blogs"] = () =>
            {
                it["returns no blogs when calling All()"] = () =>
                    retrievedBlogs.Count().should_be(0);
            };

            context["given a blog exists with title Hello World"] = () =>
            {
                before = () => new { Id = blogId, Title = "Hello World" }.InsertInto("Blogs");

                it["retrieved blogs contains inserted blog"] = () =>
                    (retrievedBlogs.First().Title as string).should_be("Hello World");

                it["contains summary"] = () =>
                    (retrievedBlogs.First().Summary as string).should_be("");
            };
        }

        void when_inserting_blog()
        {
            act = () => blogs.Insert(
                new
                {
                    Id = blogId,
                    Title = "Some Title",
                    Body = "Body"
                });

            it["inserts blog with body"] = () =>
                (blogs.Single(blogId).Body as string).should_be("Body");
        }
    }
}
