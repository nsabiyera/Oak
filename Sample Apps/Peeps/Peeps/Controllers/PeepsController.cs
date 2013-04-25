using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Peeps.Repositories;
using Oak;

namespace Peeps.Controllers
{
    public class PeepsController : BaseController
    {
        People people = new People();

        public ActionResult Index()
        {
            return Json(new { peeps = people.All() });
        }

        [HttpPost]
        public ActionResult Create(dynamic @params)
        {
            @params.peep.id = people.Insert(@params.peep);

            return Json(new { @params.peep });
        }

        [HttpPut]
        public ActionResult Update(dynamic @params)
        {
            @params.peep.id = @params.id;

            people.Save(@params.peep);

            return Json(new { @params.peep });
        }
    }
}
