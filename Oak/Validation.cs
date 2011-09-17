using System;
using System.Collections.Generic;
using System.Linq;
using Massive;
using System.Text.RegularExpressions;

namespace Oak
{
    public class MixInValidation
    {
        dynamic @this;

        List<dynamic> rules;

        List<dynamic> errors;

        public MixInValidation(DynamicModel mixWith)
        {
            rules = new List<dynamic>();

            errors = new List<dynamic>();

            mixWith.Virtual.SetMember("Errors", new DynamicEnumerableFunction(Errors));

            mixWith.Virtual.SetMember("IsValid", new DynamicFunction(IsValid));

            mixWith.Virtual.SetMember("IsPropertyValid", new Func<dynamic, dynamic>(IsValid));

            mixWith.Virtual.SetMember("FirstError", new DynamicFunction(FirstError));

            @this = mixWith;

            if (mixWith.GetType().GetMethod("Validates") != null)
            {
                IEnumerable<dynamic> validationRules = @this.Validates();

                foreach (var validationRule in validationRules)
                {
                    validationRule.Init(mixWith);

                    AddRule(validationRule);
                }    
            }
        }

        public void AddError(string property, string message)
        {
            errors.Add(new KeyValuePair<string, string>(property, message));
        }

        public void AddRule(dynamic rule)
        {
            rules.Add(rule);
        }

        public List<dynamic> Errors()
        {
            return errors;
        }

        public virtual dynamic IsValid()
        {
            return IsValid(s => true);
        }

        public virtual dynamic IsValid(dynamic property)
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
            bool isValid = rule.Validate(@this);

            if (!isValid) AddError(rule.Property, rule.Message());

            return isValid;
        }

        public string FirstError()
        {
            return errors.First().Value;
        }
    }

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

        public virtual void Init(dynamic entity) 
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
            return (entity as DynamicModel).GetMember(property);
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

        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

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

        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

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

        public override void Init(dynamic entity)
        {
            base.Init(entity as object);

            if (string.IsNullOrEmpty(Text)) Text = Property + " is taken.";
        }

        public DynamicRepository Using { get; set; }

        public bool Validate(DynamicModel entity)
        {
            object value = entity.GetMember(Property);

            if (Using.SingleWhere(Property + " = @0", value) != null) return false;

            return true;
        }
    }
}