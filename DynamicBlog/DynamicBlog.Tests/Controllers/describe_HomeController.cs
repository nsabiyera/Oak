using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using DynamicBlog.Controllers;
using System.Web.Mvc;
using Moq;
using DynamicBlog.Models;
using NUnit.Framework;
using Oak;
using Oak.Controllers;

namespace DynamicBlog.Tests.Controllers
{
    class describe_HomeController : nspec
    {
        HomeController homeController;
        ActionResult actionResult;
        SeedController seedController;
        dynamic viewBag;
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
            act = () =>
            {
                actionResult = homeController.Index();
                viewBag = (actionResult as ViewResult).ViewBag;
            };

            context["given a blog with title 'Hello', and body 'Lorem Ipsum' exists"] = () =>
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["gives a list of blogs containing a blog named 'Hello'"] = () =>
                {
                    var first = (viewBag.Blogs as IEnumerable<dynamic>).First();
                    (first.Title as string).should_be("Hello");
                    (first.Summary as string).should_be("Lorem Ipsum");
                };

                it["shows summary for each blog"] = () =>
                {
                    var first = (viewBag.Blogs as IEnumerable<dynamic>).First();
                    (first.Summary as string).should_be("Lorem Ipsum");
                };
            };
        }

        void when_navigating_to_create_a_blog()
        {
            act = () => actionResult = homeController.New() as ViewResult;

            it["returns view"] = () =>
                actionResult.should_cast_to<ViewResult>();
        }

        void when_creating_new_blog()
        {
            act = () => actionResult = homeController.New(new { title = titleToSave, body = bodyToSave });

            context["blog is valid"] = () =>
            {
                before = () =>
                {
                    titleToSave = "some blog title";
                    bodyToSave = "some body";
                };

                it["redirects to index after insert"] = () =>
                {
                    var redirectResult = actionResult.should_cast_to<RedirectToRouteResult>();
                    redirectResult.RouteValues["action"].should_be("Index");
                };

                it["inserted blog is available on home page"] = () =>
                {
                    var firstBlog = Blogs().First();
                    (firstBlog.Title as string).should_be("some blog title");
                    (firstBlog.Body as string).should_be("some body");
                };
            };

            context["invalid blog"] = () =>
            {
                before = () => 
                {
                    titleToSave = "";           	
                    bodyToSave = "some body";
                };
                    
                act = () => viewBag = (actionResult as ViewResult).ViewBag;

                it["returns view, giving user the opportunity to fix error"] = () =>
                    actionResult.should_cast_to<ViewResult>();

                it["notifies user that title is required"] = () =>
                    (viewBag.Flash as string).should_be("Title Required.");

                it["returns erroneous blog so that it can be fixed"] = () =>
                {
                    (viewBag.@params.title as string).should_be(titleToSave);
                    (viewBag.@params.body as string).should_be(bodyToSave);
                };
            };
        }

        void when_retrieving_a_blog()
        {
            act = () => actionResult = homeController.Get(new { id = 1 });

            context["given a blog post named 'Hello' with body 'Lorem Ipsum' exists"] = () => 
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["returns the blog for display"] = () =>
                {
                    var blog = (actionResult as ViewResult).ViewBag.Blog;
                    (blog.Title as string).should_be("Hello");
                    (blog.Body as string).should_be("Lorem Ipsum");
                };
            };

            context["blog doesn't exist"] = () =>
            {
                it["return 404"] = () => (actionResult as object).should_cast_to<HttpNotFoundResult>();
            };
        }

        void when_associating_comment_with_blog()
        {
            act = () => actionResult = homeController.Comment(new { id = 1, comment = "Nice blog post" });

            context["given blog exists"] = () =>
            {
                before = () => new { Title = "Hello", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                it["the comment is associated with blog post"] = () => 
                {
                    var comments = (homeController.Get(new { id = 1 }) as ViewResult).ViewBag.Comments as IEnumerable<dynamic>;
                    (comments.First().Text as string).should_be("Nice blog post");
                };

                it["redirects to blog page"] = () =>
                {
                    var routeValues = actionResult.should_cast_to<RedirectToRouteResult>().RouteValues;
                    routeValues["action"].should_be("Get");
                    routeValues["id"].should_be(1);
                };
            };
        }

        public IEnumerable<dynamic> Blogs()
        {
            return (homeController.Index() as ViewResult).ViewBag.Blogs as IEnumerable<dynamic>;
        }
    }
}
