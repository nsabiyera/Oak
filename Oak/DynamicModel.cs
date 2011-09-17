using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Massive;

namespace Oak
{
    public class DynamicModel : Prototype
    {
        public dynamic Virtual { get; set; }

        public DynamicModel()
        {
            Virtual = new Prototype();

            new MixInValidation(this);
        }

        public void Init(object o)
        {
            var dictionary = o.ToDictionary();

            foreach (var item in dictionary) SetMember(item.Key, item.Value);

            new MixInValidation(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                result = (Virtual as Prototype).GetMember(binder.Name);

                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                (Virtual as Prototype).SetMember(binder.Name, value);

                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override dynamic GetMember(string property)
        {
            if ((Virtual as Prototype).RespondsTo(property)) return (Virtual as Prototype).GetMember(property);

            return base.GetMember(property);
        }

        public override bool RespondsTo(string property)
        {
            return (Virtual as Prototype).RespondsTo(property) || base.RespondsTo(property);
        }

        public override void SetMember(string property, object value)
        {
            if((Virtual as Prototype).RespondsTo(property))
            {
                (Virtual as Prototype).SetMember(property, value);

                return;
            }

            base.SetMember(property, value);
        }

        public void Associations(Association accociation)
        {
            accociation.Init(this);
        }

        public override IEnumerable<string> Methods()
        {
            return (Virtual as Prototype).Methods().Union(base.Methods());
        }

        public override void DeleteMember(string member)
        {
            (Virtual as Prototype).DeleteMember(member);
            base.DeleteMember(member);
        }
    }
}
