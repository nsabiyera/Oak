﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                mixWith.SetUnTrackedMember("Errors", new DynamicFunction(Errors));

                mixWith.SetUnTrackedMember("IsValid", new DynamicFunction(IsValid));

                mixWith.SetUnTrackedMember("IsPropertyValid", new DynamicFunctionWithParam(IsValid));

                mixWith.SetUnTrackedMember("FirstError", new DynamicFunction(FirstError));

                IEnumerable<dynamic> validationRules = @this.Validates();

                foreach (var validationRule in validationRules)
                {
                    validationRule.Init(mixWith);

                    AddRule(validationRule);
                }
            }
        }

        public bool HasValidationCapabilities(DynamicModel mixWith)
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

        public dynamic Errors()
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
            if (rule.If != null && !rule.If(@this)) return true;

            if (rule.Unless != null && rule.Unless(@this)) return true;

            bool isValid = rule.Validate(@this);

            if (!isValid) AddError(rule.Property, rule.Message());

            return isValid;
        }

        public dynamic FirstError()
        {
            return errors.First().Value;
        }
    }

    public class Validation
    {
        public string Property { get; set; }

        public string ErrorMessage { get; set; }

        public Validation(string property)
        {
            Property = property;
        }

        public virtual void Init(dynamic entity) 
        {
            AddTrackedProperty(entity, Property);
        }

        public void AddTrackedProperty(dynamic entity, string property)
        {
            if (!entity.RespondsTo(property)) entity.SetMember(property, null);
        }

        public void AddUnTrackedProperty(dynamic entity, string property)
        {
            if (!entity.RespondsTo(property)) entity.SetUnTrackedMember(property, null);
        }

        public virtual string Message()
        {
            if (!string.IsNullOrEmpty(ErrorMessage)) return ErrorMessage;

            return Property + " is invalid.";
        }

        public dynamic PropertyValueIn(dynamic entity)
        {
            return PropertyValueIn(Property, entity);
        }

        public dynamic PropertyValueIn(string property, dynamic entity)
        {
            return entity.GetMember(property);
        }

        public Func<dynamic, bool> If { get; set; }

        public Func<dynamic, bool> Unless { get; set; }
    }

    public class Acceptance : Validation
    {
        public Acceptance(string property)
            : base(property)
        {
            Accept = true;
        }

        public dynamic Accept { get; set; }

        public bool Validate(dynamic entity)
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

        public bool Validate(dynamic entity)
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

        public bool Validate(dynamic entity)
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

        public bool Validate(dynamic entity)
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

        public bool Validate(dynamic entity)
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

            if (string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = Property + " is required.";
        }

        public bool Validate(dynamic entity)
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

            if (string.IsNullOrEmpty(ErrorMessage)) ErrorMessage = Property + " is taken.";
        }

        public DynamicRepository Using { get; set; }

        public bool Validate(dynamic entity)
        {
            object value = entity.GetMember(Property);

            var whereClause = Property + " = @0";

            var values = new List<object> { value };

            if (entity.RespondsTo("Id"))
            {
                whereClause += " and Id != @1";

                values.Add(entity.Id);
            }

            if (Using.SingleWhere(whereClause, values.ToArray()) != null) return false;

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

        public double? GreaterThan { get; set; }

        public double? GreaterThanOrEqualTo { get; set; }

        public double? EqualTo { get; set; }

        public double? LessThan { get; set; }

        public double? LessThanOrEqualTo { get; set; }

        public bool Odd { get; set; }

        public bool Even { get; set; }

        public bool Validate(dynamic entity)
        {
            string value = entity.GetMember(Property).ToString();

            var decimalValue = Double(value);

            if (decimalValue == null) return false;

            if (OnlyInteger == true && !IsInteger(value)) return false;

            if (GreaterThan != null && decimalValue <= GreaterThan) return false;

            if (GreaterThanOrEqualTo != null && decimalValue < GreaterThanOrEqualTo) return false;

            if (EqualTo != null && decimalValue != EqualTo) return false;

            if (LessThan != null && decimalValue >= LessThan) return false;

            if (LessThanOrEqualTo != null && decimalValue > LessThanOrEqualTo) return false;

            if (Odd == true && decimalValue % 2 == 0) return false;

            if (Even == true && decimalValue % 2 == 1) return false;
            
            return true;
        }

        public double? Double(string value)
        {
            double doubleResult;

            if (double.TryParse(value, out doubleResult)) return doubleResult;

            return null;
        }

        public bool IsInteger(string value)
        {
            int intResult;

            return int.TryParse(value, out intResult);
        }
    }
    
    public class Length : Validation
    {
        public Length(string property)
            : base(property)
        {
        }

        public int? Minimum { get; set; }

        public int? Maximum { get; set; }

        public IEnumerable<int> In { get; set; }

        public int? Is { get; set; }
        
        public bool IgnoreNull { get; set; }
        
        public bool Validate(DynamicModel entity)
        {
            dynamic value = entity.GetMember(Property);

            if (value == null && IgnoreNull == true) return true;

            if (value == null) return false;

            int length = value.Length;

            if (Minimum != null && length < Minimum) return false;
            
            if (Maximum != null && length > Maximum) return false;

            if (In != null && !In.Contains(length)) return false;
            
            if (Is != null && length != Is) return false;
            
            return true;
        }
    }
}