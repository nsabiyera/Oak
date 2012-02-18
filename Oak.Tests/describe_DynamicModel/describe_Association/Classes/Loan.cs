using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Loan : DynamicModel
    {
        Customers customers = new Customers();

        public Loan()
            : this(new { })
        {

        }

        public Loan(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(customers);
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email");
        }
    }
}
