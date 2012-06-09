using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using Massive;

namespace Crib.Controllers
{
    public class Consultants : DynamicRepository
    {
        	
    }

    public class RollOffsController : Controller
    {
        Consultants consultants = new Consultants();

        public dynamic Bench(string date)
        {
            return Json(consultants.All(where: "coalesce(RollOffDate, @0) <= @0", args: date));
        }

        public new JsonResult Json(object o)
        {
            return new DynamicJsonResult(o);
        }
    }
}
