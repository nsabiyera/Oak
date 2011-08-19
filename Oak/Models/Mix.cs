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

        public Mix()
            : this(new { })
        {

        }

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

            if (dictionary.ContainsKey(Capitalized(binder.Name)))
            {
                result = dictionary[Capitalized(binder.Name)];
                return true;
            }

            if (dictionary.ContainsKey(binder.Name.ToLower()))
            {
                result = dictionary[binder.Name.ToLower()];
                return true;
            }

            var fuzzyMatch = Fuzzy(dictionary, binder.Name);

            if (dictionary.ContainsKey(fuzzyMatch))
            {
                result = dictionary[fuzzyMatch];
                return true;
            }

            result = null;
            return false;
        }

        string Capitalized(string s)
        {
            return s[0].ToString().ToUpper() + s.Substring(1);
        }

        string Fuzzy(IDictionary<string, object> dictionary, string name)
        {
            foreach (var kvp in dictionary) if (kvp.Key.Equals(name, StringComparison.InvariantCultureIgnoreCase)) return kvp.Key;

            return "";
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var dictionary = MixWith as IDictionary<string, object>;

            if (dictionary.ContainsKey(binder.Name))
            {
                dictionary[binder.Name] = value;
                return true;
            }

            if (dictionary.ContainsKey(Capitalized(binder.Name)))
            {
                dictionary[Capitalized(binder.Name)] = value;
                return true;
            }

            if (dictionary.ContainsKey(binder.Name.ToLower()))
            {
                dictionary[binder.Name.ToLower()] = value;
                return true;
            }

            var fuzzyMatch = Fuzzy(dictionary, binder.Name);

            if (dictionary.ContainsKey(fuzzyMatch))
            {
                dictionary[fuzzyMatch] = value;
                return true;
            }

            return false;
        }
    }
}