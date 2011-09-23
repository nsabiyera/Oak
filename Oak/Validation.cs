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

            @this = mixWith;

            if (HasValidationCapabilities(mixWith))
            {
                mixWith.SetUnTrackedMember("Errors", new DynamicEnumerableFunction(Errors));

                mixWith.SetUnTrackedMember("IsValid", new DynamicFunction(IsValid));

                mixWith.SetUnTrackedMember("IsPropertyValid", new Func<dynamic, dynamic>(IsValid));

                mixWith.SetUnTrackedMember("FirstError", new DynamicFunction(FirstError));

                IEnumerable<dynamic> validationRules = @this.Validates();

                foreach (var validationRule in validationRules)
                {
                    validationRule.Init(mixWith);

                    AddRule(validationRule);
                }
            }
        }

        private static bool HasValidationCapabilities(DynamicModel mixWith)
        {
            return mixWith.GetType().GetMethod("Validates") != null;
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

        public Validation(string property)
        {
            Property = property;
        }

        public virtual void Init(dynamic entity) 
        {
            AddTrackedProperty(entity, Property);
        }

        public void AddTrackedProperty(DynamicModel entity, string property)
        {
            if (!entity.RespondsTo(property)) entity.SetMember(property, null);
        }

        public void AddUnTrackedProperty(DynamicModel entity, string property)
        {
            if (!entity.RespondsTo(property)) entity.SetUnTrackedMember(property, null);
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

            AddUnTrackedProperty(entity, Property + "Confirmation");
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

    public class Presence : Validation
    {
        public Presence(string property)
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
        public Uniqueness(string property, DynamicRepository usingRepository)
            : base(property)
        {
            Using = usingRepository;
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

    public class Numericality : Validation
    {
        public Numericality(string property)
            : base(property)
        {
        }

        public bool OnlyInteger { get; set; }

        public decimal? GreaterThan { get; set; }

        public decimal? GreaterThanOrEqualTo { get; set; }

        public decimal? EqualTo { get; set; }

        public decimal? LessThan { get; set; }

        public decimal? LessThanOrEqualTo { get; set; }

        public bool Odd { get; set; }

        public bool Even { get; set; }

        public bool Validate(DynamicModel entity)
        {
            string value = entity.GetMember(Property).ToString();

            int intResult;
            decimal decimalResult;
            
            if (!decimal.TryParse(value, out decimalResult))
                return false;

            if(OnlyInteger == true)
            {
                if (!int.TryParse(value, out intResult))
                    return false;
            }

            if(GreaterThan != null)
            {
                decimal.TryParse(value, out decimalResult);
                if (!(decimalResult > GreaterThan))
                    return false;
            }

            if(GreaterThanOrEqualTo != null)
            {
                decimal.TryParse(value, out decimalResult);
                if (!(decimalResult >= GreaterThanOrEqualTo))
                    return false;
            }
            
            if(EqualTo != null)
            {
                decimal.TryParse(value, out decimalResult);
                if (!(decimalResult == EqualTo))
                    return false;
            }
            
            if(LessThan != null)
            {
                decimal.TryParse(value, out decimalResult);
                if(!(decimalResult < LessThan))
                    return false;
            }
            
            if(LessThanOrEqualTo != null)
            {
                decimal.TryParse(value, out decimalResult);
                if(!(decimalResult <= LessThanOrEqualTo))
                    return false;
            }
            
            if(Odd == true)
            {
                decimal.TryParse(value, out decimalResult);
                if(decimalResult % 2 == 0)
                    return false;
            }

            if(Even == true)
            {
                decimal.TryParse(value, out decimalResult);
                if(decimalResult % 2 == 1)
                    return false;
            }
            
            return true;
        }
    }
}