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

        IDictionary<string, object> originalValues;

        IDictionary<string, object> currentValues;

        public MixInChanges(DynamicModel dynamicModel)
        {
            @this = dynamicModel;

            originalValues = new Dictionary<string, object>(dynamicModel.Hash());

            currentValues = dynamicModel.Hash();

            dynamicModel.Virtual.SetMember("IsChanged", new DynamicFunction(IsChanged));

            dynamicModel.Virtual.SetMember("Original", new Func<string, dynamic>(Original));

            dynamicModel.Virtual.SetMember("Changes", new Func<IDictionary<string, dynamic>>(Changes));

            dynamicModel.Virtual.SetMember("ChangesFor", new Func<string, dynamic>(ChangesFor));
        }

        public IDictionary<string, dynamic> Changes()
        {
            var dictionary = new Dictionary<string, dynamic>();

            foreach (var key in currentValues.Keys) dictionary.Add(key, ChangesFor(key));

            return dictionary;
        }

        public dynamic ChangesFor(string property)
        {
            return BeforeAfter(Original(property), currentValues[property]);
        }

        private dynamic BeforeAfter(dynamic before, dynamic after)
        {
            dynamic expando = new ExpandoObject();

            expando.Original = before;
            expando.New = after;

            return expando;
        }

        public dynamic Original(string property)
        {
            if (!originalValues.ContainsKey(property)) return null;

            return originalValues[property];
        }

        public dynamic IsChanged()
        {
            foreach (var item in currentValues) if (Original(item.Key) != item.Value) return true;

            return false;
        }
    }
}