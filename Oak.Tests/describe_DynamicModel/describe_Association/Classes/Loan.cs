using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Loan : DynamicModel
    {
        Customers customers;

        public Loan()
            : this(new { })
        {

        }

        public Loan(dynamic dto)
        {
            customers = new Customers();

            Init(dto);
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
