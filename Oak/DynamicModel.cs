using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;

namespace Oak
{
    public class DynamicModel : Gemini
    {
        List<string> trackedProperties = new List<string>();

        public DynamicModel(object dto)
            : base(dto)
        {

        }

        public DynamicModel()
            : this(new { })
        {

        }

        void Initialize()
        {
            Init(new Gemini(Expando));
        }

        dynamic Init()
        {
            return Init(new { });
        }

        dynamic Init(object dto)
        {
            new MixInValidation(this);

            new MixInAssociation(this);

            var dtoMembers = dto.ToDictionary().ToList();

            foreach (var item in dtoMembers) SetMember(item.Key, item.Value);

            new MixInChanges(this);

            return this;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            TrackProperty(binder.Name, value);

            return base.TrySetMember(binder, value);
        }

        public override void SetMember(string property, object value)
        {
            TrackProperty(property, value);

            base.SetMember(property, value);
        }

        public ExpandoObject TrackedProperties()
        {
            return ExpandoFor(TrackedHash());
        }

        public void TrackProperty(string property, object value)
        {
            if (value is Delegate) return;

            trackedProperties.Add(property);
        }

        public IDictionary<string, object> TrackedHash()
        {
            return HashContaining(trackedProperties);
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
