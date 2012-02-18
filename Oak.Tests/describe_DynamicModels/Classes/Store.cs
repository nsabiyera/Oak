using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Store : DynamicModel
    {
        DistributionChannels distributionChannels = new DistributionChannels();

        Warehouses warehouses = new Warehouses();

        public Store(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasOneThrough(warehouses, through: distributionChannels);
        }
    }
}
