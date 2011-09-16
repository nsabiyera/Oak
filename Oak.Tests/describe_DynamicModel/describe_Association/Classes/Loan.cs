using System;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
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
