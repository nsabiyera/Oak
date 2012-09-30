using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace TodoApp.Controllers
{
    public class ApiController : RestController
    {
        public dynamic List()
        {
            return Json(new
            {
                Todos = Url.RouteUrl(new { controller = "Todos", action = "List" })
            });
        }
    }
}
