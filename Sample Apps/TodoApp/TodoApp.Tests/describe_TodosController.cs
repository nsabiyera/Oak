using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using RestfulClient;

namespace TodoApp.Tests
{
    class describe_TodosController : _todo_app
    {
        void listing_todos()
        {
            before = () => CreateTodo();

            it["lists added todo entries"] = () =>
            {
                Todos().Count().should_be(1);
            };
        }

        dynamic todo;
        void updating_todos()
        {
            before = () =>
            {
                todo = CreateTodo();
                todo.Content = "New Description";
                Rest.Post(todo.Update, todo);
            };

            it["updates the description"] = () =>
            {
                ((object)Todos().First().Id).should_be((object)todo.Id);

                ((object)Todos().First().Content).should_be("New Description");
            };
        }

        dynamic CreateTodo()
        {
            var json = Rest.Get(TodosLink());

            var todo = Rest.Get(json.New);

            return Rest.Post(todo.Create, todo);
        }
    }
}
