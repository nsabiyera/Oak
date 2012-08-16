using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Car : DynamicModel
    {
        public Car(object dto)
            : base(dto)
        {

        }

        BluePrints bluePrints = new BluePrints();

        IEnumerable<dynamic> Associates()
        {
            yield return new HasOne(bluePrints);
        }
    }
}
