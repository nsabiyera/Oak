using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class BluePrint : DynamicModel
    {
        Cars cars = new Cars();

        public BluePrint(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(cars);
        }
    }
}
