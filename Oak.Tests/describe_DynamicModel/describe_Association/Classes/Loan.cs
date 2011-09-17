using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Loan : DynamicModel
    {
        Customers customers;

        public Loan()
        {
            customers = new Customers();

            Init();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(customers);
        }
    }
}
