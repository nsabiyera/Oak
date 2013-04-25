using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using Massive;
using Peeps.Repositories;

namespace Peeps.Controllers
{
    public class HomeController : BaseController
    {
        People people = new People();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult JQuery()
        {
            return View();
        }

        public ActionResult Backbone()
        {
            return View();
        }

        public ActionResult BackboneMarionette()
        {
            return View();
        }

        public ActionResult Knockout()
        {
            return View();
        }

        public ActionResult Angular()
        {
            return View();
        }

        public ActionResult Ember()
        {
            return View();
        }

        [HttpGet]
        public ActionResult List()
        {
            return new DynamicJsonResult(people.All());
        }

        [HttpPost]
        public ActionResult Update(dynamic @params)
        {
            if (@params.RespondsTo("Id")) people.Save(@params);

            else @params.Id = people.Insert(@params);

            return Json(@params);
        }

        [HttpPost]
        public ActionResult UpdateAll(dynamic @params)
        {
            people.Save(@params.Items);

            return new EmptyResult();
        }
    }
}
