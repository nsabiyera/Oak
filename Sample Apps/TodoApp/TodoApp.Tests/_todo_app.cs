using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using RestfulClient;
using Oak.Controllers;
using Oak;
using System.Configuration;

namespace TodoApp.Tests
{
    class _todo_app : nspec
    {
        public RestFacilitator Rest { get; set; }

        Seed testDb = new Seed(new ConnectionProfile { ConnectionString = ConfigurationManager.ConnectionStrings["TodoApp"].ConnectionString });

        Seed webDb = new Seed(new ConnectionProfile { ConnectionString = ConfigurationManager.ConnectionStrings["WebTodoApp"].ConnectionString });

        void before_each()
        {
            ResetDbs();

            Rest = new RestFacilitator("http://localhost:3000");
        }

        public void ResetDbs()
        {
            var schema = new Schema(testDb);

            schema.Seed.PurgeDb();

            schema.Scripts().ForEach<dynamic>(s => schema.Seed.ExecuteNonQuery(s()));

            schema = new Schema(webDb);

            schema.Seed.PurgeDb();

            schema.Scripts().ForEach<dynamic>(s => schema.Seed.ExecuteNonQuery(s()));
        }

        public string TodosLink()
        {
            var json = Rest.Get("/api");

            return json.Todos;
        }

        public IEnumerable<dynamic> Todos()
        {
            return Rest.Get(TodosLink()).Items as IEnumerable<dynamic>;
        }
    }
}
