using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Changes.Classes
{
    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {

        }

        public Person(dynamic dto)
        {
            Init(dto);
        }
    }
}
