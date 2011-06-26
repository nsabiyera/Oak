using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;

namespace Oak
{
    public class Mix : DynamicObject
    {
        public dynamic MixWith { get; set; }

        public Mix(object mixWith)
        {
            if (mixWith is ExpandoObject)
                MixWith = mixWith;
            else
                MixWith = mixWith.ToExpando();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var dictionary = MixWith as IDictionary<string, object>;

            if (dictionary.ContainsKey(binder.Name))
            {
                result = dictionary[binder.Name];
                return true;
            }

            result = null;
            return false;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var dictionary = MixWith as IDictionary<string, object>;

            if (dictionary.ContainsKey(binder.Name))
            {
                dictionary[binder.Name] = value;
                return true;
            }

            return false;
        }
    }
}