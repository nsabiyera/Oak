using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak.Models
{
    public class DynamicModel : Mix
    {
        public DynamicModel()
            : base(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {

        }

        public dynamic ValidationRules()
        {
            return null;
        }

        public void ValidatePresenseOf(string property)
        {

        }
    }
}