using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak
{
    public class MixInChanges
    {
        DynamicModel @this;

        IDictionary<string, dynamic> originalValues;

        IDictionary<string, dynamic> CurrentValues()
        {
            return @this.Hash();
        }

        public MixInChanges(dynamic dynamicModel)
        {
            @this = dynamicModel;

            originalValues = new Dictionary<string, object>(dynamicModel.TrackedHash());

            dynamicModel.SetMember("HasChanged", new DynamicFunctionWithParam(HasChanged));

            dynamicModel.SetMember("Original", new DynamicFunctionWithParam(Original));

            dynamicModel.SetMember("Changes", new DynamicFunctionWithParam(Changes));
        }

        public dynamic Changes(dynamic property)
        {
            if (property != null) return ChangesFor(property);

            var dictionary = new ExpandoObject() as IDictionary<string, object>;

            var keys = CurrentValues().Keys.Union(originalValues.Keys).Distinct();

            foreach (var key in keys) if (HasPropertyChanged(key)) dictionary.Add(key, ChangesFor(key));

            return dictionary;
        }

        public dynamic ChangesFor(dynamic property)
        {
            return BeforeAfter(Original(property), Current(property));
        }

        private dynamic BeforeAfter(dynamic before, dynamic after)
        {
            dynamic expando = new ExpandoObject();

            expando.Original = before;

            expando.New = after;

            return expando;
        }

        public dynamic Original(dynamic property)
        {
            return NullOrValueFor(originalValues, property);
        }

        public dynamic Current(dynamic property)
        {
            return NullOrValueFor(CurrentValues(), property);
        }

        private dynamic NullOrValueFor(IDictionary<string, dynamic> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key)) return null;

            return dictionary[key];
        }

        public dynamic HasChanged(dynamic property)
        {
            if (property != null) return HasPropertyChanged(property);

            return (Changes(property) as IDictionary<string, object>).Any();
        }

        public dynamic HasPropertyChanged(dynamic property)
        {
            return Original(property) != Current(property);
        }
    }
}