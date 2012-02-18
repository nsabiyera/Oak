using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Customer : DynamicModel
    {
        public Suppliers suppliers;

        public DistributionChannels distributionChannel;

        public Customer(object dto)
            : base(dto)
        {
            suppliers = new Suppliers();

            distributionChannel = new DistributionChannels();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasOneThrough(suppliers, through: distributionChannel);
        }
    }
}
