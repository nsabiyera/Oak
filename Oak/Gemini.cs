using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using System.Diagnostics;
using System.Reflection;

namespace Oak
{
    public static class HelpfulExtensions
    {
        public static void ForEach(this object enumerable, Action<dynamic> action)
        {
            dynamic temp = enumerable;

            foreach (var item in temp) action(item);
        }

        public static void ForEach<T>(this object enumerable, Action<T> action)
        {
            dynamic temp = enumerable;

            foreach (var item in temp) action(item);
        }
    }

    public delegate dynamic DynamicFunction();

    public delegate dynamic DynamicFunctionWithParam(dynamic parameter);

    public delegate void DynamicMethodWithParam(dynamic parameter);

    public delegate dynamic DynamicMethod();

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

            if (dto is ExpandoObject) Expando = dto;

            else Expando = dto.ToExpando();

            foreach (var method in DynamicDelegates()) AddDynamicMember(method);
        }

        private void AddDynamicMember(MethodInfo method)
        {
            var parameters = method.GetParameters().ToList();

            if (IsDynamicFunction(method, parameters)) TrySetMember(method.Name, DynamicFunctionFor(method));

            if (IsDynamicFunctionWithParam(method, parameters)) TrySetMember(method.Name, DynamicFunctionWithParamFor(method));

            if (IsDynamicMethod(method, parameters)) TrySetMember(method.Name, DynamicMethodFor(method));

            if (IsDynamicMethodWithParam(method, parameters)) TrySetMember(method.Name, DynamicMethodWithParamFor(method));
        }

        public DynamicFunction DynamicFunctionFor(MethodInfo method)
        {
            return new DynamicFunction(() => method.Invoke(this, null));
        }

        private DynamicFunctionWithParam DynamicMethodWithParamFor(MethodInfo method)
        {
            return new DynamicFunctionWithParam((arg) => 
            {
                method.Invoke(this, new[] { arg });

                return null;
            });
        }

        public DynamicFunctionWithParam DynamicFunctionWithParamFor(MethodInfo method)
        {
            return new DynamicFunctionWithParam((arg) => method.Invoke(this, new[] { arg }));
        }

        private DynamicMethod DynamicMethodFor(MethodInfo method)
        {
            return new DynamicMethod(() => method.Invoke(this, null));
        }

        public BindingFlags PrivateFlags()
        {
            return BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        }

        public  MethodInfo Method(string name)
        {
            return (this).GetType().GetMethod(name, PrivateFlags());
        }

        public  bool IsDynamicFunction(MethodInfo method, List<ParameterInfo> parameters)
        {
            if (method.ReturnType != typeof(object) && method.ReturnType != typeof(IEnumerable<dynamic>)) return false;

            if (parameters.Any()) return false;

            return true;
        }

        public bool IsDynamicFunctionWithParam(MethodInfo method, List<ParameterInfo> parameters)
        {
            if (method.ReturnType != typeof(object) && method.ReturnType != typeof(IEnumerable<dynamic>)) return false;

            if (parameters.Count != 1) return false;

            if (parameters.Any(s => s.ParameterType != typeof(object))) return false;

            return true;
        }

        public bool IsDynamicMethod(MethodInfo method, List<ParameterInfo> parameters)
        {
            if (method.ReturnType != typeof(void)) return false;

            if (parameters.Any()) return false;

            return true;
        }

        public bool IsDynamicMethodWithParam(MethodInfo method, List<ParameterInfo> parameters)
        {
            if (method.ReturnType != typeof(void)) return false;

            if (parameters.Count != 1) return false;

            if (parameters.Any(s => s.ParameterType != typeof(object))) return false;

            return true;
        }

        public IEnumerable<MethodInfo> DynamicDelegates()
        {
            return this.GetType()
                .GetMethods(PrivateFlags())
                .Where(s => IsDynamicDelegate(s, s.GetParameters().ToList()));
        }

        public bool IsDynamicDelegate(MethodInfo method, List<ParameterInfo> parameters)
        {
            return IsDynamicFunction(method, parameters) || 
                IsDynamicFunctionWithParam(method, parameters) || 
                IsDynamicMethod(method, parameters) ||
                IsDynamicMethodWithParam(method, parameters);
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

            throw new InvalidOperationException(
                "This instance of type " +
                this.GetType().Name +
                " does not respond to the property " +
                property +
                ".  These are the members that exist on this instance: " + string.Join(", ", Hash().Select(s => s.Key + " (" + s.Value.GetType().Name + ")")));
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
            return Expando as IDictionary<string, object>;
        }

        public virtual IDictionary<string, object> HashExcludingDelegates()
        {
            var dictionary = new Dictionary<string, object>();

            var delegates = Delegates();

            Hash().ForEach<KeyValuePair<string, object>>(s => 
            {
                if (!delegates.Contains(s)) dictionary.Add(s.Key, s.Value);
            });

            return dictionary;
        }

        public virtual IEnumerable<KeyValuePair<string, object>> Delegates()
        {
            return Hash().Where(s => s.Value is Delegate).ToList();
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

            if (member is DynamicFunctionWithParam)
            {
                var argsToInvokeWith = args.FirstOrDefault() as dynamic;

                if (argNames.Any()) argsToInvokeWith = GetNamedArgs(args, argNames);

                function = () => member.Invoke(argsToInvokeWith);
            };

            return function();
        }

        private dynamic GetNamedArgs(object[] args, string[] argNames)
        {
            var namedArgs = new Gemini();

            argNames.Zip(args.Skip(args.Length - argNames.Length),
                    (argName, argValue) => new
                    {
                        name = argName,
                        value = argValue
                    })
                .ForEach(arg => namedArgs.SetMember(arg.name, arg.value));

            return namedArgs;
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