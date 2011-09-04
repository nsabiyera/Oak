using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak.Models
{
    public class DynamicModel : Mix
    {
        List<KeyValuePair<string, Func<dynamic, string, bool>>> validates;

        public DynamicModel()
            : this(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {
            validates = new List<KeyValuePair<string, Func<dynamic, string, bool>>>();
        }

        public Dictionary<string, List<string>> Errors()
        {
            return null;
        }

        public void Validates(string property, Func<dynamic, string, bool> validate)
        {
            var dictionary = (MixWith as IDictionary<string, object>);

            if (!dictionary.ContainsKey(property)) dictionary.Add(property, null);

            validates.Add(new KeyValuePair<string, Func<dynamic, string, bool>>(property, validate));
        }

        public virtual bool IsValid()
        {
            bool isValid = true;

            foreach (var rule in validates)
            {
                isValid = isValid && rule.Value(this, rule.Key);
            }

            return isValid;
        }

        protected Func<dynamic, string, bool> Presense = (entity, property) =>
        {
            var dictionary = (entity.MixWith as IDictionary<string, object>);

            return !string.IsNullOrEmpty(dictionary[property] as string);
        };
    }
}