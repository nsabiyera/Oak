using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TodoApp.Repositories;
using Oak;

namespace TodoApp.Controllers
{
    public class TodosController : RestController
    {
        Todos todos = new Todos();

        public dynamic List()
        {
            return Json(new
            {
                Items = todos.All(orderBy: "[Order]").ForEach(s => ApplyLinks(s)),
                New = Url.RouteUrl(new { action = "New" })
            });
        }

        [HttpPost]
        public dynamic Create(dynamic @params)
        {
            todos.Insert(Todo(@params));

            return Get(@params.Id);
        }

        [HttpPost]
        public dynamic Update(dynamic @params)
        {
            var todo = todos.Single(@params.Id);

            todo.SetMembers(Todo(@params));

            todos.Save(todo);

            return Get(todo.Id);
        }

        [HttpPost]
        public void Delete(dynamic @params)
        {
            todos.Delete(@params.Id);
        }

        public dynamic Todo(dynamic @params)
        {
            return @params.Select("Id", "Content", "Done", "Order");
        }

        public dynamic Get(Guid id)
        {
            var todo = todos.Single(id);

            ApplyLinks(todo);

            return Json(todo);
        }

        void ApplyLinks(dynamic todo)
        {
            todo.Update = Url.RouteUrl(new { action = "Update" });

            todo.Delete = Url.RouteUrl(new { action = "Delete" });
        }

        public dynamic New()
        {
            return Json(new
            {
                Id = Guid.NewGuid(),
                Content = "",
                Create = Url.RouteUrl(new { action = "Create" })
            });
        }
    }
}
