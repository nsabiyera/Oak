using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Collections;
using System.Text.RegularExpressions;

namespace Oak.Models
{
    public class DynamicModel : Mix
    {
        List<dynamic> validates;

        public DynamicModel()
            : this(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {
            validates = new List<dynamic>();
        }

        public Dictionary<string, List<string>> Errors()
        {
            return null;
        }

        public void Validates(dynamic validate)
        {
            validate.Init(this);

            validates.Add(validate);
        }

        public virtual bool IsValid()
        {
            return IsValid(s => true);
        }

        public virtual bool IsValid(string property)
        {
            return IsValid(s => s.Property == property);
        }

        public virtual bool IsValid(Func<dynamic, bool> filter)
        {
            bool isValid = true;

            foreach (var rule in validates.Where(filter)) isValid = isValid && rule.Validate(this);

            return isValid;
        }

        bool RespondsTo(object entity, string method)
        {
            return entity.GetType().GetMethod(method) != null;
        }
    }
}
