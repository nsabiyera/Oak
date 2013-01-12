using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Oak
{
    public class Result
    {
        public bool IsCollection;
        public dynamic Value;
    }

    public class Item
    {
        public List<object> Visited;
        public bool IsCollection;
        public dynamic Self;
        public Dictionary<string, Func<dynamic>> Todo;
        private string _value;
        public string V { 
            get
            {
                if (_value == null && Todo == null)
                    return "";

                if (Todo == null)
                    return _value;

                var values = new List<string>();
                foreach (var kvp in Todo)
                {
                    values.Add(Stringify(kvp.Key) + ": " + Stringify(kvp.Value.Invoke()));
                }

                _value = "{ " + string.Join(", ", values)  + " }";

                return _value;
            }
            set { _value = value; } 
        }

        public Item(dynamic o, List<object> visitedReferences)
        {
            Self = o;
            Visited = visitedReferences;
            EnumerateProperties();
        }

        private void EnumerateProperties()
        {
            if (Visited.Contains(Self)) return;
            if (IsValueType(Self))
            {
                V = Stringify(Self);
                Todo = null;
                return;
            }

            Todo = new Dictionary<string, Func<dynamic>>();
            
            if (Self is Prototype)
            {
                Self = DynamicExtensions.ToPrototype(Self);
                foreach (var kvp in (Self as IDictionary<string, object>))
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, () => new Result { IsCollection = false, Value = kvp.Value });
                        continue;
                    }

                    IsCollection = false;
                    var todo = new Item(kvp.Value, Visited);
                    var v = todo.V;
                    Todo.Add(kvp.Key, () => new Result { Value = v, IsCollection = todo.IsCollection });
                }
                //iterate over properties resolving valuetypes, todos for others
                //return Convert(o as IDictionary<string, object>, visitedReferences);
            }
            else if (Self is IEnumerable<dynamic>)
            {
                IsCollection = true;
                var todos = new List<Item>();

                foreach (var item in (Self as IEnumerable<dynamic>))
                {
                    todos.Add(new Item(item, Visited));
                }

                _value = "[ " + string.Join(", ", todos.Select(x => x.V)) + " ]";
                Todo = null;
                //open close brackets and a list of todos?
                //return Convert(o as IEnumerable<dynamic>, visitedReferences);
            }
            else if (Self is Gemini)
            {
                IsCollection = false;

                foreach (var kvp in Self.HashOfProperties())
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = kvp.Value, IsCollection = false}));
                        continue;
                    }

                    var todo = new Item(kvp.Value, Visited);
                    var v = todo.V;
                    Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = v, IsCollection = todo.IsCollection }));
                }
                //iterate over properties resolving valuetypes, todos for others
                //return Convert(o.HashOfProperties(), visitedReferences);
            }
            else
            {
                IsCollection = false;

                foreach (var kvp in (Self as object).ToPrototype())
                {
                    if (IsValueType(kvp.Value))
                    {
                        Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = kvp.Value, IsCollection = false }));
                        continue;
                    }

                    var todo = new Item(kvp.Value, Visited);
                    var v = todo.V;
                    Todo.Add(kvp.Key, new Func<dynamic>(() => new Result { Value = v, IsCollection = todo.IsCollection }));
                }
                //iterate over properties resolving valuetypes, todos for others
                //return Convert((o as object).ToPrototype(), visitedReferences);
            }
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
            if (IsNull(o)) return "null";
            
            if (IsJsonString(o)) return "\"" + o + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

            if (IsBool(o)) return o.ToString().ToLower();

            if (o is string) return "\"" + Escape(o) + "\"";

            if (o is Result)
            {
                if (o.IsCollection == true) return o.Value;

                return Stringify(o.Value);
            }

            throw new Exception("ughhhhhhhhhhhhhhh");
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
            return Convert(o, new List<object>());
        }

        public static string Convert(dynamic o, List<object> visitedReferences)
        {
            if (visitedReferences.Contains(o)) return "";
             
            if (!IsValueType(o)) visitedReferences.Add(o);

            if (o is IEnumerable<dynamic>) return Convert(o as IEnumerable<dynamic>, visitedReferences);

            if (o is Prototype) return Convert(o as IDictionary<string, object>, visitedReferences);

            if (o is Gemini) return Convert(o.HashOfProperties(), visitedReferences);

            if (IsValueType(o)) return Stringify(o, visitedReferences);

            return Convert((o as object).ToPrototype(), visitedReferences);
        }

        private static bool IsValueType(dynamic o)
        {
            return IsJsonString(o) || IsJsonNumeric(o) || IsBool(o);
        }

        public static string Convert(IEnumerable<dynamic> o, List<object> visitedReferences)
        {
            return "[ " + string.Join(", ", 
                o.Where(s => visitedReferences.Contains(s) == false)
                .Select(s => Convert(s as object, visitedReferences))) + " ]";
        }

        public static string Convert(IDictionary<string, object> attributes, List<object> visitedReferences)
        {
            return "{ " + StringifyAttributes(attributes, visitedReferences) + " }";
        }

        private static string StringifyAttributes(IDictionary<string, object> attributes, List<object> visitedReferences)
        {
            return string.Join(", ", 
                attributes.Where(CanConvertValue)
                    .Where(kvp => visitedReferences.Contains(kvp.Value) == false)
                    .Select(kvp => StringifyAttribute(kvp, visitedReferences)));
        }

        private static string StringifyAttribute(KeyValuePair<string, object> kvp, List<object> visitedReferences)
        {
            return Stringify(kvp.Key, visitedReferences) + ": " + Stringify(kvp.Value, visitedReferences);
        }

        private static List<dynamic> ToList(dynamic enumerable)
        {
            return (enumerable as IEnumerable<dynamic>).ToList();
        }

        public static string Stringify(dynamic o, List<object> visitedReferences)
        {
            if (IsNull(o)) return "null";

            if (o is string) return "\"" + Escape(o) + "\"";

            if (IsJsonString(o)) return "\"" + o + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

            if (IsList(o)) return Convert(o as IEnumerable<dynamic>, visitedReferences);

            if (IsBool(o)) return o.ToString().ToLower();

            return Convert(o as object, visitedReferences);
        }

        private static string Escape(string o)
        {
            return o.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        public static bool IsJsonString(dynamic o)
        {
            return o is string || 
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

        public static bool IsList(dynamic o)
        {
            return o is IEnumerable<dynamic>;
        }

        public static bool IsBool(dynamic o)
        {
            return o.GetType() == typeof(bool);
        }

        public static bool CanConvertValue(KeyValuePair<string, object> kvp)
        {
            return IsNull(kvp.Value) ||
                   IsJsonString(kvp.Value) || 
                   IsJsonNumeric(kvp.Value) || 
                   IsList(kvp.Value) || 
                   IsBool(kvp.Value) ||
                   CanConvertObject(kvp.Value);
        }

        private static bool IsNull(object value)
        {
            return value == null;
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
