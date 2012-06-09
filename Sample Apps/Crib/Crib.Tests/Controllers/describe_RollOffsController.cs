using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;
using Crib.Controllers;
using Oak.Controllers;

namespace Crib.Tests.Controllers
{
    class describe_RollOffsController : nspec
    {
        RollOffsController controller;

        Schema schema;

        void before_each()
        {
            controller = new RollOffsController();

            schema = new Schema(new Seed());

            schema.Seed.PurgeDb();

            schema.Scripts().ForEach(s => schema.Seed.ExecuteNonQuery(s()));
        }

        void describe_the_bench()
        {
            before = () =>
            {
                GivenConsultant("Person 1", rollOffDate: Yesterday());

                GivenConsultant("Person 2", null);

                GivenConsultant("Person 3", rollOffDate: Tomorrow());
            };

            it["a consultant who's roll off day has passed, is on the bench"] = () =>
                Bench().Any(s => s.Name == "Person 1").should_be_true();

            it["a consultant with no roll off, is on the bench"] = () =>
                Bench().Any(s => s.Name == "Person 2").should_be_true();

            it["a consultant who's roll off date hasn't passed, is not on the bench"] = () =>
                Bench().Any(s => s.Name == "Person 3").should_be_false();
        }

        void GivenConsultant(string name, DateTime? rollOffDate = null)
        {
            new { name, rollOffDate }.InsertInto("Consultants");
        }

        IEnumerable<dynamic> Bench()
        {
            return controller.Bench(DateTime.Today.ToShortDateString()).Data;
        }

        DateTime Yesterday()
        {
            return DateTime.Today.AddDays(-1);
        }

        DateTime Tomorrow()
        {
            return DateTime.Today.AddDays(1);
        }
    }
}
