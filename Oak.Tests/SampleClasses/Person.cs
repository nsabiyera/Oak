using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.SampleClasses
{
    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {
        }

        public Person(dynamic o)
        {
            Validates(new Confirmation { Property = "Email" });

            Init(o);
        }
    }
}
