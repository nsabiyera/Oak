using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Rabbits : DynamicRepository
    {
        public Rabbits()
        {
            Projection = d => new Rabbit(d);
        }
    }
}
