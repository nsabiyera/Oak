using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using Crib.Repositories;

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
        public IEnumerable<Func<dynamic>> Scripts()
        {
            yield return CreateConsultantsTable;
        }

        public string CreateConsultantsTable()
        {
            return Seed.CreateTable("Consultants", new dynamic[] 
            {
                Seed.Id(),
                new { Name = "nvarchar(255)" },
                new { RollOffDate = "datetime" }
            });
        }

        public void SampleEntries()
        {
            var consultants = new Consultants();

            consultants.Insert(new { name = "Consultant A" });

            consultants.Insert(new { name = "Consultant B" });

            Enumerable.Range(0, 12).ForEach(i =>
            {
                consultants.Insert(new
                {
                    name = "Consultant " + i.ToString(),
                    RollOffDate = DateTime.Today.AddDays(1).AddMonths(i)
                });
            });
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
