using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class ProductWithAutoProperties : DynamicModel
    {
        public string Code { get; set; }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
        }
    }

    public class Product : DynamicModel
    {
        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
        }
    }
}
