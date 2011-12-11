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

    public delegate dynamic DynamicFunctionWithParam(dynamic parameter);

    public delegate void DynamicMethodWithParam(dynamic parameter);

    [DebuggerNonUserCode]
    public class Gemini : DynamicObject
    {
        public dynamic Expando { get; set; }

        public dynamic This()
        {
            return this as dynamic;
        }

        public Gemini()
            : this(new { })
        {

        }

        public Gemini(object dto)
        {
            if (dto == null) dto = new ExpandoObject();

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

            throw new InvalidOperationException("This instance of type " + this.GetType().Name + " does not respond to the property " + property + ".");
        }

        public virtual void SetMember(string property, object value)
        {
            TrySetMember(property, value);
        }

        public virtual void SetMembers(object o)
        {
            var dictionary = o.ToDictionary();

            foreach (var item in dictionary) SetMember(item.Key, item.Value);
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

        public virtual IEnumerable<string> Members()
        {
            return Hash().Select(s => s.Key).ToList();
        }

        public virtual IDictionary<string, object> Hash()
        {
            return (Expando as IDictionary<string, object>);
        }

        public virtual void DeleteMember(string member)
        {
            Hash().Remove(Fuzzy(Hash(), member));
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;

            if (!Hash().ContainsKey(binder.Name)) return false;

            var member = Hash()[binder.Name];

            if (!IsPolymorphicFunction(member)) return base.TryInvokeMember(binder, args, out result);

            result = InvokePolymorphicFunction(member, args, binder.CallInfo.ArgumentNames.ToArray());

            return true;
        }

        public virtual bool IsPolymorphicFunction(object member)
        {
            return (member is DynamicFunctionWithParam) || (member is DynamicMethodWithParam);
        }

        public virtual dynamic InvokePolymorphicFunction(dynamic member, object[] args, string[] argNames)
        {
            Func<dynamic> function = () =>
            {
                member.Invoke(args.FirstOrDefault());

                return null;
            };

            if (member is DynamicFunctionWithParam) function = () => member.Invoke(args.FirstOrDefault());

            return function();
        }

        dynamic ToDynamicParam(object[] args, string[] argNames)
        {
            if (AllParametersAreNamed(args, argNames))
            {
                var expando = new ExpandoObject() as IDictionary<string, object>;

                for (int i = 0; i < args.Count(); i++) expando.Add(argNames[i], args[i]);

                return expando;
            }

            return args.FirstOrDefault();
        }

        private bool AllParametersAreNamed(object[] args, IEnumerable<string> argNames)
        {
            return args.Count() == argNames.Count();
        }
    }
}