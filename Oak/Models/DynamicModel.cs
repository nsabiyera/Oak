using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;

namespace Oak.Models
{
    public class DynamicModel : Mix
    {
        public class Validation
        {
            public Validation(string property, dynamic additionalArgs, Func<dynamic, string, dynamic, bool> validationMethod)
            {
                Property = property;
                AdditionalArgs = (additionalArgs as object).ToExpando();
                ValidationMethod = validationMethod;
            }

            public string Property { get; private set; }

            dynamic AdditionalArgs { get; set; }
            
            Func<dynamic, string, dynamic, bool> ValidationMethod;

            public bool Validate(dynamic entity)
            {
                return ValidationMethod(entity, Property, AdditionalArgs);
            }
        }

        List<Validation> validates;

        public DynamicModel()
            : this(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {
            validates = new List<Validation>();
        }

        public Dictionary<string, List<string>> Errors()
        {
            return null;
        }

        public void Validates(string property, Func<dynamic, string, dynamic, bool> validate)
        {
            dynamic defaultArgs = new { };

            if (validate == Acceptance) defaultArgs = new { accept = true };

            Validates(property, validate, defaultArgs);
        }

        public void Validates(string property, Func<dynamic, string, dynamic, bool> validate, dynamic additionalArgs)
        {
            var dictionary = (MixWith as IDictionary<string, object>);

            if (!dictionary.ContainsKey(property)) dictionary.Add(property, null);

            validates.Add(new Validation(property, additionalArgs, validate));
        }

        public virtual bool IsValid()
        {
            return IsValid(s => true);
        }

        public virtual bool IsValid(string property)
        {
            return IsValid(s => s.Property == property);
        }

        public virtual bool IsValid(Func<Validation, bool> filter)
        {
            bool isValid = true;

            foreach (var rule in validates.Where(filter)) isValid = isValid && rule.Validate(this);

            return isValid;
        }

        protected Func<dynamic, string, dynamic, bool> Presense = (entity, property, additionalArgs) =>
        {
            var dictionary = (entity.MixWith as IDictionary<string, object>);

            return !string.IsNullOrEmpty(dictionary[property] as string);
        };

        protected Func<dynamic, string, dynamic, bool> Acceptance = (entity, property, additionalArgs) =>
        {
            var dictionary = (entity.MixWith as IDictionary<string, object>);

            return dictionary[property].Equals(additionalArgs.accept);
        };
    }
}