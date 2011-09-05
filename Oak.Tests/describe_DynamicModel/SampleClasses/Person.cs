using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Person : DynamicModel
    {
        public Person()
        {
            Validates(new Confirmation { Property = "Email" });
        }
    }
}
