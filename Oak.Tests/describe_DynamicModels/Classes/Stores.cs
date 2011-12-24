using System;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Stores : DynamicRepository
    {
        public Stores()
        {
            Projection = d => new Store(d);
        }
    }
}
