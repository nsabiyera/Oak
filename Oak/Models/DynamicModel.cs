using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak.Models
{
    public class DynamicModel : Mix
    {
        public class Validation
        {
            public string Property { get; set; }
            public dynamic AdditionalArgs { get; set; }
            public Func<dynamic, string, dynamic, bool> ValidationMethod;
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

            validates.Add(new Validation { Property = property, ValidationMethod = validate, AdditionalArgs = additionalArgs });
        }

        public virtual bool IsValid()
        {
            bool isValid = true;

            foreach (var rule in validates)
            {
                isValid = isValid && rule.ValidationMethod(this, rule.Property, rule.AdditionalArgs);
            }

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