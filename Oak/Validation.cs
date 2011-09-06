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
            AddDefault(entity, Property);
        }

        public void AddDefault(dynamic entity, string property)
        {
            if (!(entity as Prototype).RespondsTo(property)) (entity.Expando as IDictionary<string, object>).Add(property, null);
        }

        public void AddVirtual(dynamic entity, string property)
        {
            dynamic virtualPrototype = (entity as DynamicModel).Virtual;

            AddDefault(virtualPrototype, property);
        }

        public virtual string Message()
        {
            if (!string.IsNullOrEmpty(Text)) return Text;

            return Property + " is invalid.";
        }

        public dynamic PropertyValueIn(dynamic entity)
        {
            return PropertyValueIn(Property, entity);
        }

        public dynamic PropertyValueIn(string property, dynamic entity)
        {
            return (entity as Prototype).GetValueFor(property);
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
            return PropertyValueIn(entity).Equals(Accept);
        }
    }

    [DebuggerNonUserCode]
    public class Confirmation : Validation
    {
        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

            AddVirtual(entity, Property + "Confirmation");
        }

        public bool Validate(dynamic entity)
        {
            return PropertyValueIn(entity) == PropertyValueIn(Property + "Confirmation", entity);
        }
    }

    [DebuggerNonUserCode]
    public class Exclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return !In.Contains(PropertyValueIn(entity) as object);
        }
    }

    [DebuggerNonUserCode]
    public class Format : Validation
    {
        public string With { get; set; }

        public bool Validate(dynamic entity)
        {
            return Regex.IsMatch(PropertyValueIn(entity) as string ?? "", With);
        }
    }

    [DebuggerNonUserCode]
    public class Inclusion : Validation
    {
        public dynamic[] In { get; set; }

        public bool Validate(dynamic entity)
        {
            return In.Contains(PropertyValueIn(entity) as object);
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
            return !string.IsNullOrEmpty(PropertyValueIn(entity));
        }
    }
}