using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.Classes
{
    public class Loan : DynamicModel
    {
        Customers customers;

        public Loan()
        {
            customers = new Customers();

            Associations(new BelongsTo(customers));
        }
    }
}
