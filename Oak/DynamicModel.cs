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
    public class DynamicModel : Prototype
    {
        List<dynamic> rules;

        List<KeyValuePair<string, string>> errors;

        public dynamic Virtual { get; set; }

        public DynamicModel()
            : this(new { })
        {

        }

        public DynamicModel(object value)
            : base(value)
        {
            rules = new List<dynamic>();

            errors = new List<KeyValuePair<string, string>>();

            Virtual = new Prototype();
        }

        public void AddError(string property, string message)
        {
            errors.Add(new KeyValuePair<string, string>(property, message));
        }

        public List<KeyValuePair<string, string>> Errors()
        {
            return errors;
        }

        public void Validates(dynamic validate)
        {
            validate.Init(this);

            rules.Add(validate);
        }

        public virtual bool IsValid()
        {
            return IsValid(s => true);
        }

        public virtual bool IsValid(string property)
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

        private bool Validate(dynamic rule)
        {
            bool isValid = rule.Validate(this);

            if(!isValid) AddError(rule.Property, rule.Message());

            return isValid;
        }

        public string FirstError()
        {
            return errors.First().Value;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                result = (Virtual as Prototype).GetValueFor(binder.Name);

                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                (Virtual as Prototype).SetValueFor(binder.Name, value);

                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override dynamic GetValueFor(string property)
        {
            if ((Virtual as Prototype).RespondsTo(property))
            {
                (Virtual as Prototype).GetValueFor(property);
            }

            return base.GetValueFor(property);
        }

        public override void SetValueFor(string property, object value)
        {
            if((Virtual as Prototype).RespondsTo(property))
            {
                (Virtual as Prototype).SetValueFor(property, value);

                return;
            }

            base.SetValueFor(property, value);
        }
    }
}
