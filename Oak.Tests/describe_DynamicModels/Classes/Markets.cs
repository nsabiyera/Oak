using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Markets : DynamicRepository
    {
        public Markets()
        {
            Projection = d => new Market(d);
        }
    }
}
