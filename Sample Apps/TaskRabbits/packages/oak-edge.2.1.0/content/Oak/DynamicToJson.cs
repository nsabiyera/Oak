using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Oak
{
    public class DeserializationSession
    {
        public List<object> Visited = new List<object>();
        public bool ProcessingList;
        public Dictionary<object, string> Relatives = new Dictionary<object, string>();
    }

    public class Result
    {
        public bool ShouldStringify;
        public dynamic Value;
    }

    public class CicularReference { }

    public class Item
    {
        public Item(dynamic o, DeserializationSession session, Casing casing, bool memberOfList = false)
        {
            Self = o;
            Session = session;
            MemberOfList = memberOfList;
            Casing = casing;
        }

        public bool IsCircular()
        {
            return Value is CicularReference;
        }

        public DeserializationSession Session;
        public dynamic Self;
        public bool MemberOfList;
        public Dictionary<string, Func<dynamic>> Todo;
        public bool Enumerated;
        public bool AlreadyFound;
        public dynamic Value { get; set; }
        public Casing Casing { get; set; }
        public void Resolve()
        {
            EnumerateProperties();

            if (Session.Relatives.ContainsKey(Self)) Value = Session.Relatives[Self];

            else if (AlreadyFound) Value = new CicularReference();

            else if (Value == null && Todo == null) Value = "";

            else if (Todo != null)
            {
                var values = new List<string>();

                foreach (var kvp in Todo)
                {
                    var temp = kvp.Value.Invoke();

                    if (temp is CicularReference || temp.Value is CicularReference) continue;

                    values.Add(Stringify(kvp.Key, Casing) + ": " + Stringify(temp));
                }

                if (values.Any()) Value = "{ " + string.Join(", ", values) + " }";
            }
        }

        void ResultsFor(IDictionary<string, object> attributes)
        {
            foreach (var kvp in attributes)
            {
                var result = ResultFor(kvp);

                if (result != null) Todo.Add(kvp.Key, result);
            }
        }

        Func<dynamic> ResultFor(KeyValuePair<string, object> attribute)
        {
            if (IsValueType(attribute.Value))
            {
                return () => new Result { ShouldStringify = true, Value = attribute.Value };
            }

            return () =>
            {
                var todo = new Item(attribute.Value, Session, Casing, MemberOfList);

                todo.EnumerateProperties();

                todo.Resolve();

                var v = todo.Value;

                if (v == null || v is CicularReference) return new CicularReference();

                Cache(attribute, v);

                return new Result { ShouldStringify = false, Value = v };
            };
        }

        private void Cache(KeyValuePair<string, object> attribute, string v)
        {
            if (MemberOfList && !Session.Relatives.ContainsKey(attribute.Value)) Session.Relatives.Add(attribute.Value, v);
        }

        public void EnumerateProperties()
        {
            if (Enumerated) return;

            if (Session.Visited.Contains(Self))
            {
                AlreadyFound = true;
                return;
            }

            if (IsValueType(Self))
            {
                Value = Stringify(Self);
                Todo = null;
                Enumerated = true;
                return;
            }

            Todo = new Dictionary<string, Func<dynamic>>();

            if (Self is IEnumerable<dynamic>)
            {
                if (Session.ProcessingList == true)
                {
                    Enumerated = false;
                    return;
                }

                Session.ProcessingList = true;
                var todos = new List<Item>();

                foreach (var item in (Self as IEnumerable<dynamic>))
                {
                    Item newItem = new Item(item, Session, Casing, true);
                    newItem.EnumerateProperties();
                    todos.Add(newItem);
                }

                Session.ProcessingList = false;

                todos.ForEach(s => s.Resolve());

                if (todos.Any() && todos.All(s => s.IsCircular())) Value = new CicularReference();

                else Value = "[ " + string.Join(", ", todos.Where(s => !s.IsCircular()).Select(x => x.Value)) + " ]";

                Todo = null;
            }
            else if (Self is Prototype)
            {
                Session.Visited.Add(Self);
                ResultsFor(Self);
            }
            else if (Self is Gemini)
            {
                Session.Visited.Add(Self);
                ResultsFor(Self.HashOfProperties());
            }
            else
            {
                Session.Visited.Add(Self);
                ResultsFor((Self as object).ToPrototype());
            }

            Enumerated = true;
        }

        private static bool IsValueType(dynamic o)
        {
            return IsJsonString(o) || IsJsonNumeric(o) || IsBool(o);
        }

        public static bool IsJsonString(dynamic o)
        {
            return IsNull(o) ||
                o is string ||
                o.GetType() == typeof(DateTime) ||
                o.GetType() == typeof(Char) ||
                o.GetType() == typeof(Guid);
        }

        public static bool IsJsonNumeric(dynamic o)
        {
            return o.GetType() == typeof(Decimal) ||
                o.GetType() == typeof(int) ||
                o.GetType() == typeof(long) ||
                o.GetType() == typeof(double);
        }

        public static bool IsBool(dynamic o)
        {
            return o.GetType() == typeof(bool);
        }

        public static string Stringify(dynamic o, Casing casing = null)
        {
            casing = casing ?? new NoCasingChange();

            if (o is Result)
            {
                if (o.ShouldStringify == false) return o.Value;

                if (o.Value is string) return "\"" + Escape(o.Value) + "\"";

                return Stringify(o.Value);
            }

            if (IsNull(o)) return "null";

            if (IsJsonString(o)) return "\"" + casing.Convert(o) + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

            if (IsBool(o)) return o.ToString().ToLower();

            return "\"" + Escape(o) + "\"";
        }

        private static bool IsNull(dynamic value)
        {
            return ReferenceEquals(value, null);
        }

        private static string Escape(string o)
        {
            return o.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        public static bool IsList(dynamic o)
        {
            return o is IEnumerable<dynamic>;
        }
    }

    public class Casing
    {
        public virtual dynamic Convert(dynamic s) { return s; }
    }

    public class NoCasingChange : Casing { }

    public class CamelCasing : Casing
    {
        public override dynamic Convert(dynamic s)
        {
            return s[0].ToString().ToLower() + s.Substring(1);
        }
    }

    public class DynamicToJson
    {
        public static string Convert(dynamic o)
        {
            return Convert(o, new CamelCasing());
        }

        public static string Convert(dynamic o, Casing casing)
        {
            var session = new DeserializationSession();
            var item = new Item(o, session, casing);
            item.EnumerateProperties();
            item.Resolve();
            return item.Value;
        }

        public static bool CanConvertObject(dynamic o)
        {
            if (o is Prototype) return true;

            if (o is Gemini) return true;

            if (o is IEnumerable<object>) return (o as IEnumerable<object>).All(CanConvertObject);

            if (IsAnonymous(o)) return true;

            if (o is string) return false;

            if (o is Delegate) return false;

            return true;
        }

        //http://stackoverflow.com/questions/2483023/how-to-test-if-a-type-is-anonymous
        private static bool IsAnonymous(object o)
        {
            var type = o.GetType();

            return Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute), false)
                && type.IsGenericType && type.Name.Contains("AnonymousType")
                && (type.Name.StartsWith("<>") || type.Name.StartsWith("VB$"))
                && (type.Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic;
        }
    }
}
