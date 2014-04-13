using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Oak.Controllers
{
    /* Have all your controllers inherit from this if you'd like. */
    public class BaseController : Controller
    {
        /* Nice to have when testing, just redifine these props in your test to stub out session */
        public Func<string, object> GetSession { get; set; }
        public Action<string, object> SetSession { get; set; }

        public BaseController()
        {
            GetSession = key => HttpContext.Session[key];

            SetSession = (key, value) => HttpContext.Session[key] = value;
        }

        public new JsonResult Json(object model)
        {
            return new DynamicJsonResult(model);
        }
    }
}
