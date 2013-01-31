using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Oak;
using TaskRabbits.Repositories;
using TaskRabbits.Models;

namespace TaskRabbits.Controllers
{
    public class RabbitsController : Controller
    {
        Rabbits rabbits = new Rabbits();

        [HttpGet]
        public ActionResult Index()
        {
            var result = rabbits.All();

            result.ForEach(AddLinks);

            return new DynamicJsonResult(new
            {
                Rabbits = result,
                CreateRabbitUrl = Url.RouteUrl(new { controller = "Rabbits", action = "Create" })
            });
        }

        [HttpPost]
        public ActionResult Create(dynamic @params)
        {
            dynamic rabbit = new Rabbit(@params);

            if(rabbit.IsValid())
            {
                rabbit.Save();
                AddLinks(rabbit);
                return new DynamicJsonResult(rabbit);
            }
            else
            {
                return new DynamicJsonResult(new { Errors = rabbit.Errors() });
            }
        }

        public void AddLinks(dynamic rabbit)
        {
            rabbit.TasksUrl = Url.RouteUrl(new
            {
                controller = "Tasks",
                action = "Index",
                rabbitId = rabbit.Id
            });
        }
    }
}
