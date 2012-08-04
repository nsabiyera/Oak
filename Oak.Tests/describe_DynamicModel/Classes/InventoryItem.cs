using System;

namespace Oak.Tests.describe_DynamicModel.Classes
{
    public class InventoryItemWithAutoProps : DynamicModel
    {
        public InventoryItemWithAutoProps(object dto) : base(dto) { }

        public string Sku { get; set; }
    }

    public class InventoryItem : DynamicModel
    {
        public InventoryItem(object dto) : base(dto) { }
    }
}
