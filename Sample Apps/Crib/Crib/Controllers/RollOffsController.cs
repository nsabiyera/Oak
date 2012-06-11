using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using Massive;
using Crib.Repositories;

namespace Crib.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class RollOffsController : Controller
    {
        Consultants consultants = new Consultants();

        public dynamic Bench(string date)
        {
            return Json(consultants.Bench(date.Parse()));
        }

        public dynamic List(string benchDate)
        {
            var date = benchDate.Parse();

            dynamic consultantsForMonth = consultants.WithRollOff(date);

            var bench = consultants.Bench(date);

            return Json(consultantsForMonth.Exclude(bench));
        }

        [HttpPost]
        public void Extensions(dynamic @params)
        {
            var consultant = consultants.Single(@params.consultantId);

            consultant.RollOffDate = DateTime.Parse(@params.til);

            consultants.Save(consultant);
        }

        public new JsonResult Json(object o)
        {
            return new DynamicJsonResult(o);
        }
    }

    public static class StringExtensions
    {
        public static DateTime Parse(this string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return DateTime.Today;

            return DateTime.Parse(date);
        }
    }
}
