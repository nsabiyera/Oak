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

            yield return new Presence("Address") { Unless = d => d.IsDigitalPurchase() };
        }

        public bool PaidWithCard()
        {
            return This().PaymentType == "Card";
        }

        public bool IsDigitalPurchase()
        {
            return This().ItemType == "Digital";
        }
    }
}
