using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Product : DynamicModel
    {
        public Product()
        {
            Init();
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
        }
    }
}
