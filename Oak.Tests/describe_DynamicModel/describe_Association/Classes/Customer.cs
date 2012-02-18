using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Customer : DynamicModel
    {
        Suppliers suppliers = new Suppliers();

        DistributionChannels distributionChannel = new DistributionChannels();

        public Customer(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasOneThrough(suppliers, through: distributionChannel);
        }
    }
}
