using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Players : DynamicRepository
    {
        public Players()
        {
            Projection = d => new Player(d);
        }
    }
}
