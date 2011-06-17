using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;

namespace Oak.Controllers
{
    public class SeedController : Controller
    {
        public ConnectionProfile ConnectionProfile { get; set; }

        public Seed Seed { get; set; }

        public SeedController()
            : this(new ConnectionProfile { ConnectionString = "your connection string" })
        {

        }

        public SeedController(ConnectionProfile connectionProfile)
        {
            Seed = new Seed(connectionProfile);

            ConnectionProfile = connectionProfile;
        }

        public ActionResult Index()
        {
            return new EmptyResult();
        }

        //sample
        [HttpPost]
        public ActionResult All()
        {
            CreateBlogs();

            CreateUsers();

            return new EmptyResult();
        }

        //sample
        [HttpPost]
        public ActionResult CreateBlogs()
        {
            Seed.CommandFor("Blogs", "Create", new dynamic[] 
            { 
                new { Id = "uniqueidentifier", PrimaryKey = true },
                new { Title = "nvarchar(255)" },
                new { Body = "nvarchar(max)" }
            }).ExecuteNonQuery(ConnectionProfile);

            return new EmptyResult();
        }

        //sample
        [HttpPost]
        public ActionResult CreateUsers()
        {
            Seed.CommandFor("Users", "Create", new dynamic[] 
            { 
                new { Id = "int", PrimaryKey = true, Identity = true },
                new { Email = "nvarchar(255)" },
            }).ExecuteNonQuery(ConnectionProfile);

            return new EmptyResult();
        }

        [HttpPost]
        public ActionResult PurgeDb()
        {
            Seed.PurgeDb();

            return new EmptyResult();
        }

        //sample
        [HttpPost]
        public ActionResult SampleEntries()
        {
            new
            {
                Id = Guid.NewGuid(),
                Title = "Hello World",
                Body = "Lorem Ipsum"
            }.InsertInto("Blogs", ConnectionProfile);

            new
            {
                Email = "user@example.com"
            }.InsertInto("Users", ConnectionProfile);

            return new EmptyResult();
        }
    }
}
