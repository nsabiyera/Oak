using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using RestfulClient;

namespace TodoApp.Tests
{
    [Tag("describe_TodosController")]
    class describe_ApiController : _todo_app
    {
        void specify_a_link_to_retrieve_todo_list_is_returned()
        {
            var todos = Rest.Get(TodosLink()).Items as IEnumerable<dynamic>;

            Todos().Count().should_be(0);
        }
    }
}
