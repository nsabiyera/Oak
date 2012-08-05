using System;

namespace Oak.Tests.describe_DynamicModel.Classes
{
    public class InventoryItemWithIdAutoProp : DynamicModel
    {
        public InventoryItemWithIdAutoProp(object dto) : base(dto) { }

        public int Id { get; set; }
        public string Sku { get; set; }
    }

    public class InventoryItemWithCustomProps : DynamicModel
    {
        public InventoryItemWithCustomProps(object dto) : base(dto) { }

        public string Sku { get; set; }
        public string ColumnThatDoesntExist { get; set; }
    }

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
