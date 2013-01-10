using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace Oak
{
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
            return "[ " + string.Join(", ", o.Select(s => Convert(s as object, visitedReferences))) + " ]";
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
