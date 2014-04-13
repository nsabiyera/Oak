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
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" }, ErrorMessage = "Invalid user name." };
        }
    }

    public class RegistrationWithDeferredErrorMessage : DynamicModel
    {
        public RegistrationWithDeferredErrorMessage()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" }, ErrorMessage = new DynamicFunction(() => "Invalid user name.") };
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
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" }, ErrorMessage = "Invalid user name." };
        }
    }
}
