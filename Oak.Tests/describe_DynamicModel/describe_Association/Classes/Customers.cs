using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Customers : DynamicRepository
    {
        public Customers()
        {
            Projection = d => new Customer(d);
        }
    }
}
