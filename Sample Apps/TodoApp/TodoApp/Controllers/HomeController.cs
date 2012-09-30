using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TodoApp.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Homc/

        public ActionResult Index()
        {
            return View();
        }

    }
}
