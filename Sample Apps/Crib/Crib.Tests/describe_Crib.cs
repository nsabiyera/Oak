using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Controllers;
using Oak;

namespace Crib.Tests
{
    class describe_Crib : nspec
    {
        public Schema schema;

        void before_each()
        {
            schema = new Schema(new Seed());

            schema.Seed.PurgeDb();

            schema.Scripts().ForEach(s => schema.Seed.ExecuteNonQuery(s()));

            MvcApplication.Mixins();
        }

        public DateTime Yesterday()
        {
            return DateTime.Today.AddDays(-1);
        }

        public DateTime Tomorrow()
        {
            return DateTime.Today.AddDays(1);
        }

        public DateTime Today()
        {
            return DateTime.Today;
        }

        public DateTime NextMonth()
        {
            return DateTime.Today.AddMonths(1);
        }

        public DateTime NextYear()
        {
            return DateTime.Today.AddYears(1).AddDays(1);
        }
    }
}
