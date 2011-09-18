using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Massive;

namespace Oak
{
    public class DynamicModel : Prototype
    {
        private bool initialized;

        List<string> trackedProperties;

        List<string> untrackedProperties;

        public DynamicModel()
        {
            initialized = false;

            trackedProperties = new List<string>();

            untrackedProperties = new List<string>();
        }

        public virtual void Init()
        {
            Init(new { });
        }

        public virtual void Init(object dto)
        {
            initialized = true;

            new MixInValidation(this);

            new MixInAssociation(this);

            foreach (var item in dto.ToDictionary()) SetMember(item.Key, item.Value);

            new MixInChanges(this);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ThrowIfNotInitialized();

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ThrowIfNotInitialized();

            if(!untrackedProperties.Contains(binder.Name)) TrackProperty(binder.Name);

            return base.TrySetMember(binder, value);
        }

        public override dynamic GetMember(string property)
        {
            ThrowIfNotInitialized();

            return base.GetMember(property);
        }

        public override bool RespondsTo(string property)
        {
            ThrowIfNotInitialized();

            return base.RespondsTo(property);
        }

        public override void SetMember(string property, object value)
        {
            ThrowIfNotInitialized();

            if (!untrackedProperties.Contains(property)) TrackProperty(property);

            base.SetMember(property, value);
        }

        public virtual void SetUnTrackedMember(string property, object value)
        {
            untrackedProperties.Add(property);

            base.SetMember(property, value);
        }

        public override IEnumerable<string> Methods()
        {
            ThrowIfNotInitialized();

            return base.Methods().ToList();
        }

        public override void DeleteMember(string member)
        {
            ThrowIfNotInitialized();

            base.DeleteMember(member);
        }

        private void ThrowIfNotInitialized()
        {
            if (!initialized) throw new InvalidOperationException("DynamicModel must be initialized.  Call the Init method as the LAST statment in your constructors.");
        }

        public ExpandoObject TrackedProperties()
        {
            var expando = new ExpandoObject();

            var dictionary = expando as IDictionary<string, object>;

            foreach (var kvp in TrackedHash()) { dictionary.Add(kvp.Key, kvp.Value); }

            return expando;
        }

        public ExpandoObject UnTrackedProperties()
        {
            var expando = new ExpandoObject();

            var dictionary = expando as IDictionary<string, object>;

            Hash().Where(s => untrackedProperties.Contains(s.Key)).ToList().ForEach(s => dictionary.Add(s.Key, s.Value));

            return expando;
        }

        public void TrackProperty(string property)
        {
            trackedProperties.Add(property);
        }

        public IDictionary<string, object> TrackedHash()
        {
            var dictionary = new Dictionary<string, object>();
           
            Hash().Where(s => trackedProperties.Contains(s.Key)).ToList().ForEach(s => dictionary.Add(s.Key, s.Value));

            return dictionary;
        }
    }
}
