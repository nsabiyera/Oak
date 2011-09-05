using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Coffee : DynamicModel
    {
        public Coffee()
        {
            Validates(new Inclusion { Property = "Size", In = new[] { "small", "medium", "large" } });
        }
    }
}
