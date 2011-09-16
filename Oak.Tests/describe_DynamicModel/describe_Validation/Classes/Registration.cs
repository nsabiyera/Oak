using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Registration : DynamicModel
    {
        public Registration()
        {
            Validates(new Exclusion("UserName") { In = new[] { "admin", "administrator" } });
        }
    }
}
