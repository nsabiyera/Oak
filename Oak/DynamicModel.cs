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

        private bool initialized;

        public DynamicModel()
        {
            Virtual = new Prototype();

            initialized = false;
        }

        public void Init()
        {
            Init(new { });
        }

        public void Init(object dto)
        {
            initialized = true;

            new MixInValidation(this);

            new MixInAssociation(this);

            var dictionary = dto.ToDictionary();

            foreach (var item in dictionary) SetMember(item.Key, item.Value);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ThrowIfNotInitialized();

            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                result = (Virtual as Prototype).GetMember(binder.Name);

                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ThrowIfNotInitialized();

            if ((Virtual as Prototype).RespondsTo(binder.Name))
            {
                (Virtual as Prototype).SetMember(binder.Name, value);

                return true;
            }

            return base.TrySetMember(binder, value);
        }

        public override dynamic GetMember(string property)
        {
            ThrowIfNotInitialized();

            if ((Virtual as Prototype).RespondsTo(property)) return (Virtual as Prototype).GetMember(property);

            return base.GetMember(property);
        }

        public override bool RespondsTo(string property)
        {
            ThrowIfNotInitialized();

            return (Virtual as Prototype).RespondsTo(property) || base.RespondsTo(property);
        }

        public override void SetMember(string property, object value)
        {
            ThrowIfNotInitialized();

            if((Virtual as Prototype).RespondsTo(property))
            {
                (Virtual as Prototype).SetMember(property, value);

                return;
            }

            base.SetMember(property, value);
        }

        public override IEnumerable<string> Methods()
        {
            ThrowIfNotInitialized();

            return (Virtual as Prototype).Methods().Union(base.Methods());
        }

        public override void DeleteMember(string member)
        {
            ThrowIfNotInitialized();

            (Virtual as Prototype).DeleteMember(member);
            base.DeleteMember(member);
        }

        private void ThrowIfNotInitialized()
        {
            if (!initialized) throw new InvalidOperationException("DynamicModel must be initialized.  Call the Init method as the LAST statment in your constructors.");
        }
    }
}
