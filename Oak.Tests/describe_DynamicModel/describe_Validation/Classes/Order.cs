using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Order : DynamicModel
    {
        public Order()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("CardNumber") { If = () => _.PaidWithCard() };

            yield return new Presence("Address") { Unless = () => _.IsDigitalPurchase() };
        }

        public bool PaidWithCard()
        {
            return _.PaymentType == "Card";
        }

        public bool IsDigitalPurchase()
        {
            return This().ItemType == "Digital";
        }
    }
}
