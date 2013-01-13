using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Oak
{
    public class DeserializationSession
    {
        public List<object> Visited;
        public bool ProcessingList;
        public Dictionary<object, string> Relatives;

        public DeserializationSession()
        {
            Visited = new List<object>();
            Relatives = new Dictionary<object, string>();
        }
    }

    public class Result
    {
        public bool ShouldStringify;
        public dynamic Value;
    }
    
    public class Item
    {
        private const string AlreadyFoundGuid = "808FA4B8-889E-4BBC-9970-DA3B633A6C44";
        public DeserializationSession Session;
        public bool ShouldStringify;
        public dynamic Self;
        public bool MemberOfList;
        public Dictionary<string, Func<dynamic>> Todo;
        public bool Enumerated;
        public bool AlreadyFound;
        private string _value;
        public string Value
        {
            get
            {
                if (Enumerated == false)
                {
                    EnumerateProperties();
                }

                if (MemberOfList == true && Session.Relatives.ContainsKey(Self) == true)
                {
                    return Session.Relatives[Self];
                }

                if (AlreadyFound == true)
                    return AlreadyFoundGuid;

                if (_value == null && Todo == null)
                    return "";

                if (Todo == null)
                    return _value;

                var values = new List<string>();
                foreach (var kvp in Todo)
                {
                    var temp = kvp.Value.Invoke();
                    if (temp is Result && temp.Value is String && temp.Value == AlreadyFoundGuid)
                    {
                        continue;
                    }

                    values.Add(Stringify(kvp.Key) + ": " + Stringify(temp));
                }

                if (values.Count != 0)
                {
                    _value = "{ " + string.Join(", ", values) + " }";
                }

                return _value;
            }
            set { _value = value; }
        }

        public Item(dynamic o, DeserializationSession session)
        {
            Self = o;
            Session = session;
            EnumerateProperties();
        }

        public Item(dynamic o, DeserializationSession session, bool memberOfList)
        {
            Self = o;
            Session = session;
            MemberOfList = memberOfList;
            EnumerateProperties();
        }

        public void EnumerateProperties()
        {
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

            if (Self is Prototype)
            {
                Self = DynamicExtensions.ToPrototype(Self);
                Session.Visited.Add(Self);
                foreach (var kvp in (Self as IDictionary<string, object>))
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, () => new Result { ShouldStringify = true, Value = kvp.Value });
                        continue;
                    }
                    
                    ShouldStringify = true;
                    var todo = new Item(kvp.Value, Session, MemberOfList);
                    var v = todo.Value;
                    if (v != null)
                    {
                        if (MemberOfList == true && Session.Relatives.ContainsKey(kvp.Value) == false)
                        {
                            Session.Relatives.Add(kvp.Value, v);
                        }
                        Todo.Add(kvp.Key, () => new Result {Value = v, ShouldStringify = todo.ShouldStringify});
                    }
                }
            }
            else if (Self is IEnumerable<dynamic>)
            {
                if (Session.ProcessingList == true)
                {
                    Enumerated = false;
                    return;
                }

                Session.ProcessingList = true;
                Session.Visited.Add(Self);
                ShouldStringify = false;
                var todos = new List<Item>();

                foreach (var item in (Self as IEnumerable<dynamic>))
                {
                    todos.Add(new Item(item, Session, true));
                }

                Session.ProcessingList = false;
                _value = "[ " + string.Join(", ", todos.Select(x => x.Value)) + " ]";
                Todo = null;
            }
            else if (Self is Gemini)
            {
                Session.Visited.Add(Self);
                ShouldStringify = false;

                foreach (var kvp in Self.HashOfProperties())
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = kvp.Value, ShouldStringify = true }));
                        continue;
                    }

                    var todo = new Item(kvp.Value, Session, MemberOfList);
                    var v = todo.Value;
                    if (v != null)
                    {
                        if (MemberOfList == true && Session.Relatives.ContainsKey(kvp.Value) == false)
                        {
                            Session.Relatives.Add(kvp.Value, v);
                        }
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = v, ShouldStringify = todo.ShouldStringify }));
                    }
                }
            }
            else
            {
                Session.Visited.Add(Self);
                ShouldStringify = false;

                foreach (var kvp in (Self as object).ToPrototype())
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = kvp.Value, ShouldStringify = true }));
                        continue;
                    }

                    var todo = new Item(kvp.Value, Session, MemberOfList);
                    var v = todo.Value;
                    if (v != null)
                    {
                        if (MemberOfList == true && Session.Relatives.ContainsKey(kvp.Value) == false)
                        {
                            Session.Relatives.Add(kvp.Value, v);
                        }
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result {Value = v, ShouldStringify = todo.ShouldStringify}));
                    }
                }
            }

            Enumerated = true;
        }

        private static bool IsValueType(dynamic o)
        {
            return IsJsonString(o) || IsJsonNumeric(o) || IsBool(o);
        }

        public static bool IsJsonString(dynamic o)
        {
            return o == null ||
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

        public static string Stringify(dynamic o)
        {
            if (o is Result)
            {
                if (o.ShouldStringify == false) return o.Value;

                if (o.Value is string) return "\"" + Escape(o.Value) + "\"";

                return Stringify(o.Value);
            }

            if (IsNull(o)) return "null";

            if (IsJsonString(o)) return "\"" + o + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

            if (IsBool(o)) return o.ToString().ToLower();

            return "\"" + Escape(o) + "\"";
        }

        private static bool IsNull(object value)
        {
            return value == null;
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

    public class DynamicToJson
    {
        public static string Convert(dynamic o)
        {
            var session = new DeserializationSession();
            var item = new Item(o, session);
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
