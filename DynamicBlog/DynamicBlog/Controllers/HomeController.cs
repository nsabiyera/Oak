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

        public HomeController()
        {
            Blogs = new Blogs();
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

        public ActionResult Get(dynamic @params)
        {
            var blog = Blogs.Single(@params.id);

            if (blog == null) return HttpNotFound();

            ViewBag.Blog = blog;

            ViewBag.Comments = blog.Comments();

            return View();
        }

        [HttpPost]
        public ActionResult New(dynamic @params)
        {
            var blog = new Blog(new
            {
                Title = @params.title,
                Body = @params.body
            });

            if (!blog.IsValid())
            {
                ViewBag.Flash = blog.Validate();
                ViewBag.@params = @params;
                return View();
            }

            Blogs.Save(blog);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult Comment(dynamic @params)
        {
            var blog = Blogs.Single(@params.id);

            blog.AddComment(@params.comment);

            return RedirectToAction("Get", new { id = @params.id });
        }

        public ActionResult Edit(dynamic @params)
        {
            var blog = Blogs.Single(@params.id);

            if (blog == null) return HttpNotFound();

            ViewBag.Blog = blog;

            return View();
        }

        public ActionResult Update(dynamic @params)
        {
            var blog = Blogs.Single(@params.id);

            blog.Title = @params.title;
            blog.Body = @params.body;

            if(!blog.IsValid())
            {
                ViewBag.Flash = blog.Validate();
                ViewBag.Blog = blog;
                return View("edit");
            }

            Blogs.Save(blog);

            return RedirectToAction("get", new { id = @params.id });
        }
    }
}



