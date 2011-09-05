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
        List<dynamic> rules;
        List<KeyValuePair<string, string>> errors;

        public DynamicModel()
            : this(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {
            rules = new List<dynamic>();
            errors = new List<KeyValuePair<string, string>>();
        }

        public void AddError(string property, string message)
        {
            errors.Add(new KeyValuePair<string, string>(property, message));
        }

        public List<KeyValuePair<string, string>> Errors()
        {
            return errors;
        }

        public void Validates(dynamic validate)
        {
            validate.Init(this);

            rules.Add(validate);
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
            errors.Clear();

            bool isValid = true;

            foreach (var rule in rules.Where(filter)) isValid = Validate(rule) && isValid;

            return isValid;
        }

        public bool Validate(dynamic rule)
        {
            bool isValid = rule.Validate(this);

            if(!isValid) AddError(rule.Property, rule.Message());

            return isValid;
        }

        bool RespondsTo(object entity, string method)
        {
            return entity.GetType().GetMethod(method) != null;
        }

        public string FirstError()
        {
            return errors.First().Value;
        }
    }
}
