using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using Massive;
using System.Diagnostics;

namespace Oak
{
    public delegate dynamic DynamicFunction();

    public delegate IEnumerable<dynamic> DynamicEnumerableFunction();

    public class Prototype : DynamicObject
    {
        public dynamic Expando { get; set; }

        public Prototype()
            : this(new { })
        {

        }

        public Prototype(object dto)
        {
            if (dto is ExpandoObject)
                Expando = dto;
            else
                Expando = dto.ToExpando();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetMember(binder.Name, out result);
        }

        public virtual bool RespondsTo(string property)
        {
            object result = null;

            return TryGetMember(property, out result);
        }

        public bool TryGetMember(string name, out object result)
        {
            var dictionary = Hash();

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

        public virtual dynamic GetMember(string property)
        {
            object result = null;

            if (TryGetMember(property, out result)) return result;

            throw new InvalidOperationException("This prototype does not respond to the property " + property + ".");
        }

        public virtual void SetMember(string property, object value)
        {
            TrySetMember(property, value);
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
            return TrySetMember(binder.Name, value);
        }

        public bool TrySetMember(string property, object value)
        {
            var dictionary = Hash();

            if (dictionary.ContainsKey(property))
            {
                dictionary[property] = value;
                return true;
            }

            if (dictionary.ContainsKey(Capitalized(property)))
            {
                dictionary[Capitalized(property)] = value;
                return true;
            }

            if (dictionary.ContainsKey(property.ToLower()))
            {
                dictionary[property.ToLower()] = value;
                return true;
            }

            var fuzzyMatch = Fuzzy(dictionary, property);

            if (dictionary.ContainsKey(fuzzyMatch))
            {
                dictionary[fuzzyMatch] = value;
                return true;
            }

            dictionary.Add(property, value);

            return true;
        }

        public virtual IEnumerable<string> Methods()
        {
            return Hash().Select(s => s.Key).ToList();
        }

        public IDictionary<string, object> Hash()
        {
            return (Expando as IDictionary<string, object>);
        }

        public virtual void DeleteMember(string member)
        {
            Hash().Remove(Fuzzy(Hash(), member));
        }
    }
}