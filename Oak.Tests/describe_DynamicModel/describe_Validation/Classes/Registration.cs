using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Registration : DynamicModel
    {
        public IEnumerable<dynamic> Validates()
        {
            yield return new Exclusion("UserName") { In = new[] { "admin", "administrator" } };
        }
    }
}
