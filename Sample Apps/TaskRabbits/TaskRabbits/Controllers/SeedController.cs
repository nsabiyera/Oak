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
        public IEnumerable<Func<dynamic>> Scripts()
        {
            yield return CreateRabbitsTable;

            yield return CreateTasksTable;
        }

        public string CreateRabbitsTable()
        {
            return Seed.CreateTable("Rabbits",
                Seed.Id(),
                new { Name = "nvarchar(255)" }
            );
        }

        public string CreateTasksTable()
        {
            return Seed.CreateTable("Tasks",
                Seed.Id(),
                new { RabbitId = "int", ForeignKey = "Rabbits(Id)" },
                new { Description = "nvarchar(255)" },
                new { DueDate = "datetime" }
            );
        }

        public void SampleEntries()
        {
            var rabbitId = new { Name = "Yours Truly" }.InsertInto("Rabbits");

            new { rabbitId, Description = "bolt onto vans", DueDate = DateTime.Today }.InsertInto("Tasks");

            rabbitId = new { Name = "Hiro Protaganist" }.InsertInto("Rabbits");

            new { rabbitId, Description = "save the world", DueDate = DateTime.Today }.InsertInto("Tasks");

            new { rabbitId, Description = "deliver pizza", DueDate = DateTime.Today }.InsertInto("Tasks");

            rabbitId = new { Name = "Lots" }.InsertInto("Rabbits");

            for (int i = 0; i < 1000; i++)
            {
                new { rabbitId, Description = "Task: " + i.ToString(), DueDate = DateTime.Today }.InsertInto("Tasks");
            }
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
            DebugBootStrap.SkipInefficientQueryDetectionForThisRequest();

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
