using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Oak.Controllers
{
    public class LocalOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.RequestContext.HttpContext.Request.IsLocal == false)
            {
                filterContext.Result = new HttpNotFoundResult();
            }
        }
    }

    [LocalOnly]
    public class SeedController : Controller
    {
        //it uses the same connection profile class
        //that all Massive uses
        public ConnectionProfile ConnectionProfile { get; set; }

        //this is part of oak
        public Seed Seed { get; set; }

        public SeedController()
        {
            Seed = new Seed();
        }

        //the default controller action does nothing
        public ActionResult Index() { return new EmptyResult(); }

        //performing an http post to All() recreates the schema
        //it assumes that no schema already exists
        [HttpPost]
        public ActionResult All()
        {
            CreateBlogs(); //this is a sample

            CreateComments();

            return new EmptyResult();
        }

        //this creates the blogs table using the Seed DSL
        //sample
        public void CreateBlogs()
        {
            //use seed to create a sql command
            string createCommand = Seed.CreateTable(
                "Blogs", //the table
                new dynamic[] //the schema
                { 
                    new { Id = "int", PrimaryKey = true, Identity = true },
                    new { Title = "nvarchar(255)" },
                    new { Body = "nvarchar(max)" }
                });

            createCommand.ExecuteNonQuery();
        }

        public void CreateComments()
        {
            string createCommand = Seed.CreateTable("Comments", new dynamic[] 
            { 
                new { Id = "int", PrimaryKey = true, Identity = true },
                new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                new { Text = "nvarchar(max)" }
            });

            createCommand.ExecuteNonQuery();
        }

        //performing an http post to PurgeDb will purge the database
        [HttpPost]
        public ActionResult PurgeDb()
        {
            Seed.PurgeDb();

            return new EmptyResult();
        }

        //sample entries go here, we'll get to this later
        [HttpPost]
        public ActionResult SampleEntries()
        {
            var blogPosts =
                new[] 
                { 
                    new { Title = "My First Blog Post", Body = "First Body" },
                    new { Title = "Another Blog Post", Body = "Another Body"  } ,
                    new { Title = "Sample", Body = "Sample Body" } ,
                    new { Title = "Yet Another One", Body = "Yet Another Body" }
                };

            foreach (var blog in blogPosts)
            {
                new
                {
                    Title = blog.Title,
                    Body = blog.Body
                }.InsertInto("Blogs");
            }

            return new EmptyResult();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = Content(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }
    }
}
