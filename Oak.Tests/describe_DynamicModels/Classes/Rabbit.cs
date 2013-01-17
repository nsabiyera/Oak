using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Rabbit : DynamicModel
    {
        public Rabbit(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Validates()
        {
            yield return new Presence("Name");
        }
    }
}
