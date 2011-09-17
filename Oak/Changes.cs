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

        IDictionary<string, dynamic> currentValues;

        public MixInChanges(DynamicModel dynamicModel)
        {
            @this = dynamicModel;

            originalValues = new Dictionary<string, object>(dynamicModel.Hash());

            currentValues = dynamicModel.Hash();

            dynamicModel.Virtual.SetMember("HasChanged", new DynamicFunction(HasChanged));

            dynamicModel.Virtual.SetMember("HasPropertyChanged", new DynamicFuctionWithParam(HasPropertyChanged));

            dynamicModel.Virtual.SetMember("Original", new DynamicFuctionWithParam(Original));

            dynamicModel.Virtual.SetMember("Changes", new DynamicFunction(Changes));

            dynamicModel.Virtual.SetMember("ChangesFor", new DynamicFuctionWithParam(ChangesFor));
        }

        public dynamic Changes()
        {
            var dictionary = new Dictionary<string, dynamic>();

            var keys = currentValues.Keys.Union(originalValues.Keys).Distinct();

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
            return NullOrValueFor(currentValues, property);
        }

        private dynamic NullOrValueFor(IDictionary<string, dynamic> dictionary, string key)
        {
            if (!dictionary.ContainsKey(key)) return null;

            return dictionary[key];
        }

        public dynamic HasChanged()
        {
            return Changes().Count > 0;
        }

        public dynamic HasPropertyChanged(dynamic property)
        {
            return Original(property) != Current(property);
        }
    }
}