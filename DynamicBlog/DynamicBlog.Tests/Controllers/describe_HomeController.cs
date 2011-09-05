using System;
using System.Collections.Generic;
using System.Linq;
using NSpec;
using DynamicBlog.Controllers;
using System.Web.Mvc;
using Oak;
using Oak.Controllers;

namespace DynamicBlog.Tests.Controllers
{
    class describe_HomeController : nspec
    {
        HomeController homeController;
        SeedController seedController;
        dynamic action;
        string titleToSave;
        string bodyToSave;

        void before_each()
        {
            seedController = new SeedController();
            seedController.PurgeDb();
            seedController.All();
            homeController = new HomeController();
        }

        void navigating_to_the_home_page()
        {
            act = () => action = homeController.Index();

            context["given a blog with title 'Hello', and body 'Lorem Ipsum' exists"] = () =>
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["blog list has blog named 'Hello'"] = () =>
                {
                    (FirstBlog().Title as string).should_be("Hello");
                };

                it["blog has summary"] = () =>
                {
                    (FirstBlog().Summary as string).should_be("Lorem Ipsum");
                };
            };
        }

        void when_creating_new_blog()
        {
            act = () => action = homeController.New(new { Title = titleToSave, Body = bodyToSave });

            context["blog is valid"] = () =>
            {
                before = () =>
                {
                    titleToSave = "some blog title";
                    bodyToSave = "some body";
                };

                it["redirects to index after insert"] = () =>
                {
                    (action.RouteValues["action"] as string).should_be("Index");
                };

                it["inserted blog is available on home page"] = () =>
                {
                    (FirstBlog().Title as string).should_be("some blog title");
                    (FirstBlog().Body as string).should_be("some body");
                };
            };

            context["invalid blog"] = () =>
            {
                before = () => 
                {
                    titleToSave = "";           	
                    bodyToSave = "some body";
                };
                    
                act = () => 
                {
                    action = action.ViewBag;
                };

                it["notifies user that there were validation errors"] = () =>
                    (action.Flash as string).should_be("Title is required.");
            };
        }

        void when_retrieving_a_blog()
        {
            act = () => action = homeController.Get(new { id = 1 });

            context["given a blog post named 'Hello' with body 'Lorem Ipsum' exists"] = () => 
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["returns the blog for display"] = () =>
                {
                    var blog = action.ViewBag.Blog;
                    (blog.Title as string).should_be("Hello");
                    (blog.Body as string).should_be("Lorem Ipsum");
                };
            };

            context["blog doesn't exist"] = () =>
            {
                it["return 404"] = () => (action is HttpNotFoundResult).should_be_true();
            };
        }

        void when_associating_comment_with_blog()
        {
            act = () => action = homeController.Comment(new { id = 1, comment = "Nice blog post" });

            context["given blog exists"] = () =>
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["the comment is associated with blog post"] = () => 
                {
                    var comments = homeController.Get(new { id = 1 }).ViewBag.Comments as IEnumerable<dynamic>;
                    (comments.First().Text as string).should_be("Nice blog post");
                };

                it["redirects to blog page"] = () =>
                {
                    var routeValues = action.RouteValues;
                    (routeValues["action"] as string).should_be("Get");
                    ((int)routeValues["id"]).should_be(1);
                };
            };
        }

        void when_requesting_to_update_blog()
        {
            act = () => action = homeController.Edit(new { id = 1 });

            context["given blog 'Hello'"] = () =>
            {
                before = () => new { Title = "Hello" }.InsertInto("Blogs");

                it["returns view with blog"] = () =>
                {
                    var blog = action.ViewBag.Blog;
                    (blog.Title as string).should_be("Hello");
                };
            };

            context["blog doesn't exist"] = () =>
            {
                it["returns 404"] = () =>
                    (action is HttpNotFoundResult).should_be_true();
            };
        }

        void when_updating_blog()
        {
            act = () => action = homeController.Update(new { id = 1, title = titleToSave, body = "new body" });

            context["given blog Hello and valid update"] = () =>
            {
                before = () =>
                {
                    titleToSave = "new title";
                    new { Title = "Hello" }.InsertInto("Blogs");
                };

                it["updates blog with new title and body"] = () =>
                {
                    var blog = Blog(1);
                    (blog.Title as string).should_be("new title");
                    (blog.Body as string).should_be("new body");
                };

                it["redirects to get"] = () =>
                {
                    var values = action.RouteValues;
                    (values["action"] as string).should_be("get");
                    ((int)values["id"]).should_be(1);
                };

                context["invalid blog update"] = () =>
                {
                    before = () => titleToSave = "";

                    it["returns errors stating that Title Required."] = () =>
                        (action.ViewBag.Flash as string).should_be("Title is required.");

                    it["returns blog with values that were attempted"] = () =>
                    {
                        (action.ViewBag.Blog.Body as string).should_be("new body");
                        (action.ViewBag.Blog.Title as string).should_be("");
                    };
                };
            };
        }

        IEnumerable<dynamic> Blogs()
        {
            return (homeController.Index() as ViewResult).ViewBag.Blogs as IEnumerable<dynamic>;
        }

        dynamic Blog(int id)
        {
            return (homeController.Get(new { id }) as dynamic).ViewBag.Blog;
        }

        dynamic FirstBlog()
        {
            return Blogs().First();
        }
    }
}
