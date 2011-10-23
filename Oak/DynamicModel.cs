using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace Oak
{
    public class DynamicModel : Gemini
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

        public virtual dynamic Init()
        {
            return Init(new { });
        }

        public virtual dynamic Init(object dto)
        {
            initialized = true;

            new MixInValidation(this);

            new MixInAssociation(this);

            foreach (var item in dto.ToDictionary()) SetMember(item.Key, item.Value);

            new MixInChanges(this);

            return this;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            ThrowIfNotInitialized();

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            ThrowIfNotInitialized();

            TrackProperty(binder.Name);

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

            TrackProperty(property);

            base.SetMember(property, value);
        }

        public virtual void SetUnTrackedMember(string property, object value)
        {
            untrackedProperties.Add(property);

            base.SetMember(property, value);
        }

        public override IEnumerable<string> Members()
        {
            ThrowIfNotInitialized();

            return base.Members().ToList();
        }

        public override void DeleteMember(string member)
        {
            ThrowIfNotInitialized();

            base.DeleteMember(member);
        }

        private void ThrowIfNotInitialized()
        {
            if (!initialized) throw new InvalidOperationException("DynamicModel must be initialized.  Call the Init method as the LAST statement in your constructors.");
        }

        public ExpandoObject TrackedProperties()
        {
            return ExpandoFor(TrackedHash());
        }

        public ExpandoObject UnTrackedProperties()
        {
            return ExpandoFor(UnTrackedHash());
        }

        public void TrackProperty(string property)
        {
            if (!untrackedProperties.Contains(property)) trackedProperties.Add(property);
        }

        public IDictionary<string, object> TrackedHash()
        {
            return HashContaining(trackedProperties);
        }

        public IDictionary<string, object> UnTrackedHash()
        {
            return HashContaining(untrackedProperties);
        }

        private ExpandoObject ExpandoFor(IDictionary<string, object> dictionary)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            foreach (var kvp in dictionary) { expando.Add(kvp.Key, kvp.Value); }

            return expando as ExpandoObject;
        }

        private IDictionary<string, object> HashContaining(IEnumerable<string> keys)
        {
            var dictionary = new Dictionary<string, object>();

            Hash().Where(s => keys.Contains(s.Key)).ToList().ForEach(s => dictionary.Add(s.Key, s.Value));

            return dictionary;
        }
    }
}
