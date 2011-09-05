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
    public class Validation
    {
        public string Property { get; set; }

        public virtual void Init(dynamic entity) 
        {
            var dictionary = (entity.MixWith as IDictionary<string, object>);

            if (!dictionary.ContainsKey(Property)) AddDefault(entity, Property);
        }

        public void AddDefault(dynamic entity, string property)
        {
            (entity.MixWith as IDictionary<string, object>).Add(property, null);
        }
    }

    public class Acceptance : Validation
    {
        public Acceptance()
        {
            Accept = true;
        }

        public dynamic Accept { get; set; }

        public bool Validate(dynamic entity)
        {
            return (entity.MixWith as IDictionary<string, object>)[Property].Equals(Accept);
        }
    }

    public class Confirmation : Validation
    {
        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

            AddDefault(entity, Property + "Confirmation");
        }

        public bool Validate(dynamic entity)
        {
            var dictionary = (entity.MixWith as IDictionary<string, object>);

            return dictionary[Property].Equals(dictionary[Property + "Confirmation"]);
        }
    }

    public class Exclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return !In.Contains((entity.MixWith as IDictionary<string, object>)[Property]);
        }
    }

    public class Format : Validation
    {
        public string With { get; set; }

        public bool Validate(dynamic entity)
        {
            return Regex.IsMatch((entity.MixWith as IDictionary<string, object>)[Property] as string, With);
        }
    }

    public class Inclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return In.Contains((entity.MixWith as IDictionary<string, object>)[Property]);
        }
    }

    public class Presense : Validation
    {
        public bool Validate(dynamic entity)
        {
            return !string.IsNullOrEmpty((entity.MixWith as IDictionary<string, object>)[Property] as string);
        }
    }
}