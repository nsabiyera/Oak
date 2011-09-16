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
    public class Validation
    {
        public string Property { get; set; }

        public string Text { get; set; }

        public Validation()
        {
            
        }

        public Validation(string property)
        {
            Property = property;
        }

        public virtual void Init(DynamicModel entity) 
        {
            AddDefault(entity, Property);
        }

        public void AddDefault(Prototype entity, string property)
        {
            if (!entity.RespondsTo(property)) (entity.Expando as IDictionary<string, object>).Add(property, null);
        }

        public void AddVirtual(DynamicModel entity, string property)
        {
            dynamic virtualPrototype = entity.Virtual;

            AddDefault(virtualPrototype, property);
        }

        public virtual string Message()
        {
            if (!string.IsNullOrEmpty(Text)) return Text;

            return Property + " is invalid.";
        }

        public dynamic PropertyValueIn(DynamicModel entity)
        {
            return PropertyValueIn(Property, entity);
        }

        public dynamic PropertyValueIn(string property, DynamicModel entity)
        {
            return (entity as DynamicModel).GetValueFor(property);
        }
    }

    public class Acceptance : Validation
    {
        public Acceptance(string property)
            : base(property)
        {
            Accept = true;
        }

        public dynamic Accept { get; set; }

        public bool Validate(DynamicModel entity)
        {
            return PropertyValueIn(entity).Equals(Accept);
        }
    }

    public class Confirmation : Validation
    {
        public Confirmation(string property)
            : base(property)
        {
            
        }

        public override void Init(DynamicModel entity)
        {
            base.Init(entity);

            AddVirtual(entity, Property + "Confirmation");
        }

        public bool Validate(DynamicModel entity)
        {
            return PropertyValueIn(entity) == PropertyValueIn(Property + "Confirmation", entity);
        }
    }

    public class Exclusion : Validation
    {
        public Exclusion(string property)
            : base(property)
        {
            
        }
        public dynamic[] In { get; set; }

        public bool Validate(DynamicModel entity)
        {
            return !In.Contains(PropertyValueIn(entity) as object);
        }
    }

    public class Format : Validation
    {
        public Format(string property)
            : base(property)
        {
            
        }

        public string With { get; set; }

        public bool Validate(DynamicModel entity)
        {
            return Regex.IsMatch(PropertyValueIn(entity) as string ?? "", With);
        }
    }

    public class Inclusion : Validation
    {
        public Inclusion(string property)
            : base(property)
        {
            
        }

        public dynamic[] In { get; set; }

        public bool Validate(DynamicModel entity)
        {
            return In.Contains(PropertyValueIn(entity) as object);
        }
    }

    public class Presense : Validation
    {
        public Presense(string property)
            : base(property)
        {
            
        }

        public override void Init(DynamicModel entity)
        {
            base.Init(entity);

            if (string.IsNullOrEmpty(Text)) Text = Property + " is required.";
        }

        public bool Validate(DynamicModel entity)
        {
            return !string.IsNullOrEmpty(PropertyValueIn(entity));
        }
    }

    public class Uniqueness : Validation
    {
        public Uniqueness(string property)
            : base(property)
        {
            
        }

        public override void Init(DynamicModel entity)
        {
            base.Init(entity);

            if (string.IsNullOrEmpty(Text)) Text = Property + " is taken.";
        }

        public DynamicRepository Using { get; set; }

        public bool Validate(DynamicModel entity)
        {
            object value = entity.GetValueFor(Property);

            if (Using.SingleWhere(Property + " = @0", value) != null) return false;

            return true;
        }
    }
}