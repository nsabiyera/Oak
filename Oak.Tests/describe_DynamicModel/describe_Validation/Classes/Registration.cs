using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Registration : DynamicModel
    {
        public Registration()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" } };
        }
    }

    public class RegistrationWithAutoProperties : DynamicModel
    {
        public string UserName { get; set; }

        public RegistrationWithAutoProperties()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" } };
        }
    }
}
