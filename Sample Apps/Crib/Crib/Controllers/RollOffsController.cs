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
    public class RollOffsController : Controller
    {
        Consultants consultants = new Consultants();

        public dynamic Bench(string date)
        {
            date = DateTime.Today.ToShortDateString() ?? date;

            return Json(consultants.Bench(DateTime.Parse(date)));
        }

        public dynamic List(string benchDate)
        {
            benchDate = DateTime.Today.ToShortDateString() ?? benchDate;

            var date = DateTime.Parse(benchDate);

            var consultantsForMonth = consultants.WithRollOff(date);

            var bench = consultants.Bench(date);

            return Json(consultantsForMonth.Where(s => !bench.Any(b => b.Id == s.Id)));
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
}
