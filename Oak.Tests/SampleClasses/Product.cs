using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.SampleClasses
{
    public class Product : DynamicModel
    {
        public Product()
        {
            Validates(new Format { Property = "Code", With = @"\A[a-zA-z]+\z" });
        }
    }
}
