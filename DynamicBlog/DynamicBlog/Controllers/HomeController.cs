using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DynamicBlog.Models;

namespace DynamicBlog.Controllers
{
    public class HomeController : Controller
    {
        public Blogs Blogs { get; set; }

        public HomeController(Blogs blogs)
        {
            Blogs = blogs;
        }

        public ActionResult Index()
        {
            ViewBag.Blogs = Blogs.All();

            return View();
        }

        public ActionResult New()
        {
            return View();
        }

        [HttpPost]
        public ActionResult New(string title, string body)
        {
            var blog = new Blog(new
            {
                Id = Guid.NewGuid(),
                Title = title,
                Body = body
            });

            if (!blog.IsValid())
            {
                ViewBag.Flash = blog.Validate();
                return View();
            }

            Blogs.Insert(blog);

            return RedirectToAction("Index");
        }
    }
}



