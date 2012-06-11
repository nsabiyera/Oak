using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;
using Crib.Models;

namespace Crib.Repositories
{
    public class Consultants : DynamicRepository
    {
        public Consultants()
        {
            Projection = d => new Consultant(d);
        }

        public IEnumerable<dynamic> Bench(DateTime date)
        {
            var consultants = All(where: "coalesce(RollOffDate, @0) <= @0", args: date);

            consultants.ForEach(s => s.OnBench = true);

            return consultants;
        }

        public IEnumerable<dynamic> WithRollOff(DateTime start, DateTime? end = null)
        {
            end = end ?? DateTime.MaxValue;

            return All(where: "RollOffDate between @0 and @1", args: new object[] { start, end });
        }
    }
}