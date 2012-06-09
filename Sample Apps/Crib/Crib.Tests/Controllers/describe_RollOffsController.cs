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
                Bench().has("Person 1");

            it["a consultant with no roll off, is on the bench"] = () =>
                Bench().has("Person 2");

            it["a consultant who's roll off date hasn't passed, is not on the bench"] = () =>
                Bench().doesnt_have("Person 3");
        }

        void describe_rolling_for_year()
        {
            before = () =>
            {
                GivenConsultant("Person 1", rollOffDate: Tomorrow());

                GivenConsultant("Person 2", rollOffDate: NextMonth());

                GivenConsultant("Person 3", rollOffDate: NextYear());

                GivenConsultant("Person 4", rollOffDate: Yesterday());

                GivenConsultant("Person 5", null);
            };

            it["includes consultants that roll off this month"] = () =>
                List().has("Person 1");

            it["includes consultants rolling of next month (or any dates after)"] = () =>
            {
                List().has("Person 2");

                List().has("Person 3");
            };

            it["the list does not contain anyone on the bench"] = () =>
            {
                List().doesnt_have("Person 4");

                List().doesnt_have("Person 5");
            };
        }

        void GivenConsultant(string name, DateTime? rollOffDate = null)
        {
            new { name, rollOffDate }.InsertInto("Consultants");
        }

        IEnumerable<dynamic> Bench()
        {
            return controller.Bench(Today().ToShortDateString()).Data;
        }

        IEnumerable<dynamic> List()
        {
            return controller.List(Today().ToShortDateString()).Data;
        }

        DateTime Yesterday()
        {
            return DateTime.Today.AddDays(-1);
        }

        DateTime Tomorrow()
        {
            return DateTime.Today.AddDays(1);
        }

        DateTime Today()
        {
            return DateTime.Today;
        }

        DateTime NextMonth()
        {
            return DateTime.Today.AddMonths(1);
        }

        DateTime NextYear()
        {
            return DateTime.Today.AddYears(1).AddDays(1);
        }
    }

    public static class ConsultantsAssertions
    {
        public static dynamic has(this IEnumerable<dynamic> consultants, string name)
        {
            if (!consultants.Any(s => s.Name == name)) throw new InvalidOperationException(name + " was not found in consultant list.");

            return consultants.First(s => s.Name == name);
        }

        public static void doesnt_have(this IEnumerable<dynamic> consultants, string name)
        {
            if (consultants.Any(s => s.Name == name)) throw new InvalidOperationException(name + " was found in consultant list.");
        }
    }
}
