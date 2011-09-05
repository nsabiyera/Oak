using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Registration : DynamicModel
    {
        public Registration()
        {
            Validates(new Exclusion { Property = "UserName", In = new[] { "admin", "administrator" } });
        }
    }
}
