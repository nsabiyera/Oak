using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace Oak
{
    public class DynamicModel : Gemini
    {
        private object dto;

        private bool initialized;

        List<string> trackedProperties;

        List<string> untrackedProperties;

        public DynamicModel(object dto)
        {
            this.dto = dto;

            initialized = false;

            trackedProperties = new List<string>();

            untrackedProperties = new List<string>();
        }

        public DynamicModel()
            : this(new { })
        {

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
            InitIfNeeded();

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            InitIfNeeded();

            TrackProperty(binder.Name, value);

            return base.TrySetMember(binder, value);
        }

        public override dynamic GetMember(string property)
        {
            InitIfNeeded();

            return base.GetMember(property);
        }

        public override bool RespondsTo(string property)
        {
            InitIfNeeded();

            return base.RespondsTo(property);
        }

        public override void SetMember(string property, object value)
        {
            InitIfNeeded();

            TrackProperty(property, value);

            base.SetMember(property, value);
        }

        public virtual void SetUnTrackedMember(string property, object value)
        {
            untrackedProperties.Add(property);

            base.SetMember(property, value);
        }

        public override IEnumerable<string> Members()
        {
            InitIfNeeded();

            return base.Members().ToList();
        }

        public override void DeleteMember(string member)
        {
            InitIfNeeded();

            base.DeleteMember(member);
        }

        public virtual dynamic InitIfNeeded()
        {
            if (!initialized) Init(dto);

            return this;
        }

        public ExpandoObject TrackedProperties()
        {
            InitIfNeeded();

            return ExpandoFor(TrackedHash());
        }

        public ExpandoObject UnTrackedProperties()
        {
            InitIfNeeded();

            return ExpandoFor(UnTrackedHash());
        }

        public void TrackProperty(string property, object value)
        {
            if (value is Delegate) return;

            if (!untrackedProperties.Contains(property)) trackedProperties.Add(property);
        }

        public IDictionary<string, object> TrackedHash()
        {
            InitIfNeeded();

            return HashContaining(trackedProperties);
        }

        public IDictionary<string, object> UnTrackedHash()
        {
            InitIfNeeded();

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

        public dynamic Select(params string[] args)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            args.ForEach(s => expando.Add(s, GetMember(s)));

            return new Gemini(expando);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            InitIfNeeded();

            return base.TryInvokeMember(binder, args, out result);
        }

        public override IDictionary<string, object> Hash()
        {
            InitIfNeeded();

            return base.Hash();
        }
    }
}
