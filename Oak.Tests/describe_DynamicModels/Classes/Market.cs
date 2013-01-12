using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Market : DynamicModel
    {
        Suppliers suppliers = new Suppliers();

        SupplyChains supplyChains = new SupplyChains();

        public Market(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(supplyChains);

            yield return new HasManyThrough(suppliers, supplyChains);
        }
    }
}