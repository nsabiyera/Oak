using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;

namespace Oak.Controllers
{
    public class LocalOnly : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if(filterContext.RequestContext.HttpContext.Request.IsLocal == false)
            {
                filterContext.Result = new HttpNotFoundResult();
            }
        }
    }

    [LocalOnly]
    public class SeedController : Controller
    {
        public Seed Seed { get; set; }

        public SeedController()
        {
            Seed = new Seed();
        }

        public ActionResult Index()
        {
            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult All()
        {
            CreateBlogs();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult CreateBlogs()
        {
            Seed.CreateTable("Blogs", new dynamic[] 
            { 
                new { Id = "uniqueidentifier", PrimaryKey = true },
                new { Title = "nvarchar(255)" },
                new { Body = "nvarchar(max)" }
            }).ExecuteNonQuery();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult PurgeDb()
        {
            Seed.PurgeDb();

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult SampleEntries()
        {
            new
            {
                Id = Guid.NewGuid(),
                Title = "Hello World",
                Body = "Lorem Ipsum"
            }.InsertInto("Blogs");

            return new EmptyResult();
        }
    }
}
