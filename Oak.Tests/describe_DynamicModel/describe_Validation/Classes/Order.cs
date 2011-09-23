using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Order : DynamicModel
    {
        public Order()
        {
            Init();
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("CardNumber") { If = d => d.PaidWithCard() };
        }

        public bool PaidWithCard()
        {
            return This().PaymentType == "Card";
        }
    }
}
