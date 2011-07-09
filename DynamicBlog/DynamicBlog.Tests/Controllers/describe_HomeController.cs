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

namespace DynamicBlog.Tests.Controllers
{
    class describe_HomeController : nspec
    {
        HomeController homeController;
        ActionResult actionResult;
        dynamic viewBag;
        Mock<Blogs> blogs;
        List<dynamic> blogsFromModel;
        string titleToSave;
        string bodyToSave;
        List<dynamic> insertedBlogs;

        void before_each()
        {
            blogs = new Mock<Blogs>();
            blogsFromModel = new List<dynamic> { new { } };
            blogs.Setup(s => s.All()).Returns(blogsFromModel);
            homeController = new HomeController(blogs.Object);
            insertedBlogs = new List<dynamic>();
            blogs.Setup(s => s.Insert(It.IsAny<object>())).Callback<object>(b => insertedBlogs.Add(b));
        }

        void navigating_to_the_home_page()
        {
            act = () =>
            {
                actionResult = homeController.Index();
                viewBag = (actionResult as ViewResult).ViewBag;
            };

            it["gives a list of blogs"] = () =>
                (viewBag.Blogs as IEnumerable<dynamic>).should_be(blogsFromModel);
        }

        void when_navigating_to_create_a_blog()
        {
            act = () => actionResult = homeController.New() as ViewResult;

            it["returns view"] = () =>
                actionResult.should_cast_to<ViewResult>();
        }

        void when_creating_new_blog()
        {
            act = () => actionResult = homeController.New(titleToSave, bodyToSave);

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

                it["inserts blog"] = () =>
                {
                    insertedBlogs.Any(s =>
                            s.Title == "some blog title" &&
                            s.Id != Guid.Empty &&
                            s.Body == "some body")
                        .should_be_true();
                };
            };

            context["invalid blog"] = () =>
            {
                before = () => titleToSave = "";

                act = () => viewBag = (actionResult as ViewResult).ViewBag;

                it["returns view, giving user the opportunity to fix error"] = () =>
                    actionResult.should_cast_to<ViewResult>();

                it["notifies user that title is required"] = () =>
                    (viewBag.Flash as string).should_be("Title Required.");
            };
        }
    }
}
