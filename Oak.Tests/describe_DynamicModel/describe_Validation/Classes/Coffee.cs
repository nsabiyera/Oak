using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class CoffeeWithAutoProperties : DynamicModel
    {
        public string Size { get; set; }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Inclusion("Size") { In = new[] { "small", "medium", "large" } };
        }
    }

    public class Coffee : DynamicModel
    {
        public IEnumerable<dynamic> Validates()
        {
            yield return new Inclusion("Size") { In = new[] { "small", "medium", "large" } };
        }
    }
}
