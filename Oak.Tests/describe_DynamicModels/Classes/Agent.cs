using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Agent : DynamicModel
    {
        Booths booths = new Booths();

        public Agent(object dto)
            : base(dto)
        {

        }

        public Agent()
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(booths) { IdColumnOfParentTable = "BoothId" };
        }
    }
}
