using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Agents : DynamicRepository
    {
        public Agents()
        {
            Projection = d => new Agent(d);
        }
    }
}
