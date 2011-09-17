using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Product : DynamicModel
    {
        public IEnumerable<dynamic> Validates()
        {
            yield return new Format("Code") { With = @"\A[a-zA-z]+\z" };
        }
    }
}
