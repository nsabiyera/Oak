using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using Newtonsoft.Json;

namespace TodoApp.Controllers
{
    public class RestController : Controller
    {
        public new dynamic Json(object o)
        {
            return new DynamicJsonResult(o);
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            filterContext.ExceptionHandled = true;

            filterContext.HttpContext.Response.StatusCode = 500;
            filterContext.Result = new DynamicJsonResult(new 
            {
                Error = Bullet.ScrubStackTrace(filterContext.Exception.ToString())
            });
        }
    }
}
