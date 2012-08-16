using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{   
    public class BluePrints : DynamicRepository
    {
        public BluePrints()
        {
            Projection = d => new BluePrint(d);
        }
    }
}
