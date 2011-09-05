using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Collections;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Oak
{
    [DebuggerNonUserCode]
    public class Validation
    {
        public string Property { get; set; }

        public string Text { get; set; }

        public virtual void Init(dynamic entity) 
        {
            if (!(entity as Mix).RespondsTo(Property)) AddDefault(entity, Property);
        }

        public void AddDefault(dynamic entity, string property)
        {
            (entity.MixWith as IDictionary<string, object>).Add(property, null);
        }

        public virtual string Message()
        {
            if (!string.IsNullOrEmpty(Text)) return Text;

            return Property + " is invalid.";
        }
    }

    [DebuggerNonUserCode]
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

    [DebuggerNonUserCode]
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

    [DebuggerNonUserCode]
    public class Exclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return !In.Contains((entity.MixWith as IDictionary<string, object>)[Property]);
        }
    }

    [DebuggerNonUserCode]
    public class Format : Validation
    {
        public string With { get; set; }

        public bool Validate(dynamic entity)
        {
            return Regex.IsMatch((entity.MixWith as IDictionary<string, object>)[Property] as string ?? "", With);
        }
    }

    [DebuggerNonUserCode]
    public class Inclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return In.Contains((entity.MixWith as IDictionary<string, object>)[Property]);
        }
    }

    [DebuggerNonUserCode]
    public class Presense : Validation
    {
        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

            if (string.IsNullOrEmpty(Text)) Text = Property + " is required.";
        }

        public bool Validate(dynamic entity)
        {
            return !string.IsNullOrEmpty((entity as Mix).GetValueFor(Property));
        }
    }
}