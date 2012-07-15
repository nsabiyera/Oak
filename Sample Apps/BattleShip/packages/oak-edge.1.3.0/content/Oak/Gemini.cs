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
        private bool initialized;

        private static List<KeyValuePair<Type, Func<dynamic, dynamic>>> Includes = new List<KeyValuePair<Type, Func<dynamic, dynamic>>>();

        private static List<KeyValuePair<Type, Action<dynamic>>> MethodHooks = new List<KeyValuePair<Type, Action<dynamic>>>();

        private List<Type> types = new List<Type>();

        private static List<KeyValuePair<Type, Action<dynamic>>> ClassHooks = new List<KeyValuePair<Type, Action<dynamic>>>();

        private List<Type> extendedWith = new List<Type>();

        private static Dictionary<Type, List<MethodInfo>> ReflectionCache = new Dictionary<Type, List<MethodInfo>>();

        public virtual List<Type> ExtendedWith()
        {
            return extendedWith;
        }

        public dynamic Expando { get; set; }

        public static void Extend<A, B>()
        {
            Includes.Add(new KeyValuePair<Type, Func<dynamic, dynamic>>(typeof(A), (i) =>
            {
                i.Extend<B>();

                return null;
            }));
        }

        public static void Extend<T>(Func<dynamic, dynamic> extension)
        {
            Includes.Add(new KeyValuePair<Type, Func<dynamic, dynamic>>(typeof(T), extension));
        }

        public static void Extend<T>(Action<dynamic> extension)
        {
            Includes.Add(new KeyValuePair<Type, Func<dynamic, dynamic>>(typeof(T), (i) => { extension(i); return null; }));
        }

        public static void Initialized<T>(Action<dynamic> callback)
        {
            ClassHooks.Add(new KeyValuePair<Type, Action<dynamic>>(typeof(T), callback));
        }

        public static void MethodDefined<T>(Action<dynamic> extension)
        {
            MethodHooks.Add(new KeyValuePair<Type, Action<dynamic>>(typeof(T), extension));
        }

        protected dynamic _
        {
            get { return this as dynamic; }
        }

        public dynamic This()
        {
            return _;
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

            foreach (var method in DynamicDelegates(this.GetType())) AddDynamicMember(method);

            var currentType = this.GetType();

            while (currentType != typeof(object))
            {
                types.Add(currentType);

                currentType = currentType.BaseType;
            }

            var includes = Includes.Where(s => types.Contains(s.Key));

            foreach (var include in includes)
            {
                object result = (include.Value(this) as object);

                if (result == null) continue;

                (result.ToExpando() as IDictionary<string, object>).ToList().ForEach(s => SetMember(s.Key, s.Value));
            }

            initialized = false;
        }

        private void AddDynamicMember(MethodInfo method)
        {
            var parameters = method.GetParameters().ToList();

            if (IsDynamicFunction(method, parameters)) TrySetMember(method.Name, DynamicFunctionFor(method));

            else if (IsDynamicFunctionWithParam(method, parameters)) TrySetMember(method.Name, DynamicFunctionWithParamFor(method));

            else if (IsDynamicMethod(method, parameters)) TrySetMember(method.Name, DynamicMethodFor(method));

            else if (IsDynamicMethodWithParam(method, parameters)) TrySetMember(method.Name, DynamicFunctionWithParamFor(method));
        }

        public DynamicFunction DynamicFunctionFor(MethodInfo method)
        {
            return new DynamicFunction(() => Invoke(method, null));
        }

        public DynamicFunctionWithParam DynamicFunctionWithParamFor(MethodInfo method)
        {
            return new DynamicFunctionWithParam((arg) => Invoke(method, new[] { arg }));
        }

        private DynamicMethod DynamicMethodFor(MethodInfo method)
        {
            return new DynamicMethod(() => Invoke(method, null));
        }

        private object Invoke(MethodInfo method, object[] parameters)
        {
            try { return method.Invoke(this, parameters); }

            catch (Exception ex) { throw ex.InnerException; }
        }

        public BindingFlags PrivateFlags()
        {
            return BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;
        }

        public MethodInfo Method(string name)
        {
            return (this).GetType().GetMethod(name, PrivateFlags());
        }

        public bool IsDynamicFunction(MethodInfo method, List<ParameterInfo> parameters)
        {
            if (method.ReturnType != typeof(object) && method.ReturnType != typeof(IEnumerable<dynamic>)) return false;

            if (parameters.Any()) return false;

            return true;
        }

        public void Extend<T>() where T : class
        {
            var constructor = typeof(T).GetConstructor(new Type[] { typeof(object) });

            constructor.Invoke(new object[] { this });

            extendedWith.Add(typeof(T));
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

        public IEnumerable<MethodInfo> DynamicDelegates(Type type)
        {
            if (type == typeof(Gemini) || type == typeof(object)) return new List<MethodInfo>();

            if (ReflectionCache.ContainsKey(type)) return ReflectionCache[type];

            var delegates = type
                .GetMethods(PrivateFlags())
                .Where(s => IsDynamicDelegate(s, s.GetParameters().ToList())).ToList();

            delegates.AddRange(DynamicDelegates(type.BaseType));

            ReflectionCache.Add(type, delegates);

            return delegates;
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
            InitializeIfNeeded(name);

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

        void InitializeIfNeeded(string property)
        {
            if (initialized) return;

            initialized = true;

            var hooks = ClassHooks.Where(s => types.Contains(s.Key));

            foreach (var hook in hooks) hook.Value(this);
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
                ".  These are the members that exist on this instance: " + __Info__());
        }

        public virtual dynamic __Info__()
        {
            return GeminiInfo.Parse(this);
        }

        public virtual void SetMember(string property, object value, bool suppress)
        {
            TrySetMember(property, value, suppress);
        }

        public virtual void SetMember(string property, object value)
        {
            TrySetMember(property, value, suppress: false);
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
            return TrySetMember(binder.Name, value, suppress: false);
        }

        public bool TrySetMember(string property, object value, bool suppress = false)
        {
            InitializeIfNeeded(property);

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

            if (!suppress)
            {
                var hooks = MethodHooks.Where(s => s.Key == this.GetType());

                foreach (var hook in hooks) hook.Value(new Gemini(new { Name = property, Instance = this })); //not under test yet...
            }

            return true;
        }

        public virtual IEnumerable<string> Members()
        {
            return Hash().Select(s => s.Key).ToList();
        }

        public virtual IDictionary<string, object> Hash()
        {
            InitializeIfNeeded("Hash");

            return Expando as IDictionary<string, object>;
        }

        public virtual IDictionary<string, object> HashExcludingDelegates()
        {
            var dictionary = new ExpandoObject() as IDictionary<string, object>;

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
            InitializeIfNeeded(binder.Name);

            result = null;

            if (!RespondsTo(binder.Name)) return MethodMissing(binder, args, ref result);

            var member = GetMember(binder.Name);

            if (!DynamicFunction(member)) return base.TryInvokeMember(binder, args, out result);

            result = InvokeDynamicFunction(member, args, binder.CallInfo.ArgumentNames.ToArray());

            return true;
        }

        public virtual bool MethodMissing(InvokeMemberBinder binder, object[] args, ref object result)
        {
            if (RespondsTo("MethodMissing"))
            {
                var argNames = binder.CallInfo.ArgumentNames.ToArray();

                result = _.MethodMissing(
                    new Gemini(new
                    {
                        Name = binder.Name,
                        Parameters = args,
                        ParameterNames = argNames,
                        Instance = this,
                        Parameter = GetNamedArgs(args, argNames)
                    }));

                return true;
            }

            return false;
        }

        public virtual bool DynamicFunction(object member)
        {
            return (member is DynamicFunctionWithParam) || (member is DynamicMethodWithParam);
        }

        public virtual dynamic InvokeDynamicFunction(dynamic member, object[] args, string[] argNames)
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

            argNames.Zip(
                args.Skip(args.Length - argNames.Length),
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

        public virtual dynamic Select(params string[] args)
        {
            var expando = new ExpandoObject() as IDictionary<string, object>;

            args.ForEach(s => expando.Add(s, GetMember(s)));

            return new Gemini(expando);
        }

        public virtual dynamic Exclude(params string[] args)
        {
            var expando = (Hash() as IDictionary<string, object>).ToList();

            var dictionary = new ExpandoObject() as IDictionary<string, object>;

            args = args.Select(s => s.ToLower()).ToArray();

            expando.ForEach(s =>
            {
                if (!args.Contains(s.Key.ToLower())) dictionary.Add(s.Key, GetMember(s.Key));
            });

            return new Gemini(dictionary);
        }

        public virtual bool IsOfType<T>()
        {
            return TypeExtensions.IsOfType<T>(this);
        }

        public virtual bool IsOfKind<T>()
        {
            return TypeExtensions.IsOfKind<T>(this) || ExtendedWith().Contains(typeof(T));
        }

        public override string ToString()
        {
            return __Info__();
        }
    }
}