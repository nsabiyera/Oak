using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Diagnostics;

namespace Oak
{
    [DebuggerNonUserCode]
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
            return TryGetMember(binder.Name, out result);
        }

        public bool RespondsTo(string property)
        {
            object result = null;

            return TryGetMember(property, out result);
        }

        public bool TryGetMember(string name, out object result)
        {
            var dictionary = MixWith as IDictionary<string, object>;

            if (dictionary.ContainsKey(name))
            {
                result = dictionary[name];
                return true;
            }

            if (dictionary.ContainsKey(Capitalized(name)))
            {
                result = dictionary[Capitalized(name)];
                return true;
            }

            if (dictionary.ContainsKey(name.ToLower()))
            {
                result = dictionary[name.ToLower()];
                return true;
            }

            var fuzzyMatch = Fuzzy(dictionary, name);

            if (dictionary.ContainsKey(fuzzyMatch))
            {
                result = dictionary[fuzzyMatch];
                return true;
            }

            result = null;
            return false;
        }

        public dynamic GetValueFor(string property)
        {
            object result = null;

            if (TryGetMember(property, out result)) return result;

            throw new InvalidOperationException("This mix does not respond to the property " + property + ".");
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