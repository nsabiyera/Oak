using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Cars : DynamicRepository
    {
        public Cars()
        {
            Projection = d => new Car(d);
        }
    }
}
