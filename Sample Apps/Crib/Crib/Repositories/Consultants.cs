using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

namespace Crib.Repositories
{
    public class Consultants : DynamicRepository
    {
        public IEnumerable<dynamic> Bench(DateTime date)
        {
            return All(where: "coalesce(RollOffDate, @0) <= @0", args: date);
        }

        public IEnumerable<dynamic> WithRollOff(DateTime start, DateTime? end = null)
        {
            end = end ?? DateTime.MaxValue;

            return All(where: "RollOffDate between @0 and @1", args: new object[] { start, end });
        }
    }
}