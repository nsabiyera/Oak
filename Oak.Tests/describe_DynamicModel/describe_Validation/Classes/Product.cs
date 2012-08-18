using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class ProductWithAutoProperties : DynamicModel
    {
        public string Code { get; set; }
        public int ProductId { get; set; }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
            yield return new Format("ProductId") { With = @"\d{6}" };
        }
    }

    public class Product : DynamicModel
    {
        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
            yield return new Format("ProductId") { With = @"\d{6}" };
        }
    }
}
