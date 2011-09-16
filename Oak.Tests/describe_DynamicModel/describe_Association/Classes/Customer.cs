using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Customer : DynamicModel
    {
        Suppliers suppliers;

        DistributionChannels distributionChannel;

        public Customer(dynamic dto)
        {
            suppliers = new Suppliers();

            distributionChannel = new DistributionChannels();

            Associations(new HasOne(suppliers) { Through = distributionChannel });

            Init(dto);
        }
    }
}
