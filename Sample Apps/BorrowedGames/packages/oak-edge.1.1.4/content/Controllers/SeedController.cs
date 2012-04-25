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
            if (!RunningLocally(filterContext)) filterContext.Result = new HttpNotFoundResult();
        }

        public bool RunningLocally(ActionExecutingContext filterContext)
        {
            return filterContext.RequestContext.HttpContext.Request.IsLocal;
        }
    }

    public class Schema
    {
        /// <summary>
        /// Change this method to create your tables.  Take a look 
        /// at each method, CreateSampleTable(), CreateAnotherSampleTable(), 
        /// AlterSampleTable() and AdHocChange()...you'll want to replace 
        /// this with your own set of methods.
        /// </summary>
        public IEnumerable<Func<string>> Scripts()
        {
            yield return CreateSampleTable;

            yield return CreateAnotherSampleTable;

            yield return AlterSampleTable;
        }

        //here is a sample of how to create a table
        public string CreateSampleTable()
        {
            return Seed.CreateTable("SampleTable", new dynamic[] 
            { 
                new { Id = "uniqueidentifier", PrimaryKey = true },
                new { Foo = "nvarchar(max)", Default = "Hello" },
                new { Bar = "int", Nullable = false }
            });
        }

        //here is another sample of how to create a table
        public string CreateAnotherSampleTable()
        {
            return Seed.CreateTable("AnotherSampleTable", new dynamic[] 
            { 
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Foo = "nvarchar(max)", Default = "Hello", Nullable = false },
            });
        }

        //here is a sample of how to alter a table
        public string AlterSampleTable()
        {
            return Seed.AddColumns("SampleTable", new dynamic[] 
            {
                new { AnotherColumn = "bigint" },
                new { YetAnotherColumn = "nvarchar(max)" }
            });
        }

        //different ad hoc queries
        public IEnumerable<string> AdHocChange()
        {
            //hey look, you can just do an ad hoc read
            var reader = "select * from SampleTable".ExecuteReader();

            while (reader.Read())
            {
                //do stuff here like yield return strings
            }

            var name = "select top 1 name from sysobjects".ExecuteScalar() as string;

            yield return "drop table SampleTable";

            yield return "drop table AnotherSampleTable";
        }

        public void SampleEntries()
        {
            new
            {
                Id = Guid.NewGuid(),
                Title = "Hello World",
                Body = "Lorem Ipsum"
            }.InsertInto("Blogs");
        }

        public Seed Seed { get; set; }

        public Schema(Seed seed) { Seed = seed; }
    }

    [LocalOnly]
    public class SeedController : Controller
    {
        public Seed Seed { get; set; }

        public Schema Schema { get; set; }

        public SeedController()
        {
            Seed = new Seed();

            Schema = new Schema(Seed);
        }

        [HttpPost]
        public ActionResult PurgeDb()
        {
            Seed.PurgeDb();

            return new EmptyResult();
        }

        /// <summary>
        /// Execute this command to write all the scripts to sql files.
        /// </summary>
        [HttpPost]
        public ActionResult Export()
        {
            var exportPath = Server.MapPath("~");

            Seed.Export(exportPath, Schema.Scripts());

            return Content("Scripts executed to: " + exportPath);
        }

        [HttpPost]
        public ActionResult All()
        {
            Schema.Scripts().ForEach<dynamic>(s => Seed.ExecuteNonQuery(s()));

            return new EmptyResult();
        }

        /// <summary>
        /// Create sample entries for your database in this method.
        /// </summary>
        [HttpPost]
        public ActionResult SampleEntries()
        {
            Schema.SampleEntries();

            return new EmptyResult();
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.Result = Content(filterContext.Exception.Message);
            filterContext.ExceptionHandled = true;
        }
    }
}
