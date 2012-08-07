using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;

namespace Oak.Controllers
{
    /*
     * Hi there.  This is where you define the schema for your database.  The nuget package rake-dot-net
     * has two commands that hook into this class.  One is 'rake reset' and the other is 'rake sample'.
     * The 'rake reset' command will drop all tables and regen your schema.  The 'rake sample' command
     * will drop all tables, regen your schema and insert sample data you've specified.  To get started
     * update the Scripts() method in the class below.
     */
    public class Schema
    {
        /// <summary>
        /// Change this method to create your tables.  Take a look 
        /// at each method, CreateSampleTable(), AlterSampleTable() and AdHocChange()...
        /// you'll want to replace this with your own set of methods.
        /// </summary>
        public IEnumerable<Func<dynamic>> Scripts()
        {
            yield return CreateSampleTable;

            yield return AlterSampleTable;

            yield return AdHocChange;
        }

        //here is a sample of how you create a table
        //use Seed's CreateTable method
        //when you create a new method, make sure you return it
        //from the Scripts method body
        public string CreateSampleTable()
        {
            return Seed.CreateTable("SampleTable", new dynamic[] 
            { 
                //create anonymous types for each column you want
                new { Id = "int", Identity = true, PrimaryKey = true },
                //here are other ways you can create a primary key column
                //or new { Id = "uniqueidentifier", PrimaryKey = true },
                //or Seed.Id(),
                //or Seed.GuidId(),

                //here is how you would create a column named Foo with a default value
                new { Foo = "nvarchar(max)", Default = "Hello" },

                //here is how you would create a nullable column
                new { Bar = "int", Nullable = false }
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

        //you can run a suite of ad hoc queries by
        //creating a method that returns an IEnumerable of string.
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
        }

        //here is how you would create sample entries, if you have
        //the rake-dot-net nuget package, you can run the command
        //'rake sample' to generate these sample entries
        public void SampleEntries()
        {
            new
            {
                Title = "Hello World",
                Body = "Lorem Ipsum"
            }.InsertInto("Blogs");
        }

        public Seed Seed { get; set; }

        public Schema(Seed seed) { Seed = seed; }
    }


    //this class is the hook into the Schema class
    //the PurgeDb(), All(), Export(), and SampleEntires() controller
    //actions are all accessible via rake-dot-net
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
}
