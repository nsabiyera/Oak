using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Changes.Classes
{
    public class PersonWithAutoProps : DynamicModel
    {
        public PersonWithAutoProps(object dto)
            : base(dto)
        {

        }

        public PersonWithAutoProps()
        {

        }

        public string FirstName { get; set; }

        public string OnlyGettable
        {
            get
            {
                return "only gettable";
            }
        }
    }

    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {

        }

        public Person(object dto)
            : base(dto)
        {
        }
    }
}
