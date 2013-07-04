using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;

namespace TaskRabbits.Controllers
{
    public class ApiController : Controller
    {
        public ActionResult Index()
        {
            return new DynamicJsonResult(new
            {
                GetRabbitsUrl = Url.RouteUrl(new
                {
                    controller = "Rabbits",
                    action = "Index"
                })
            });
        }
    }
}
