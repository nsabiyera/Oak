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
            if (o is IEnumerable<dynamic>) return Convert(o as IEnumerable<dynamic>);

            if(o is ExpandoObject) return Convert(o as IDictionary<string, object>);

            if (o is Gemini) return Convert(PublicAndDynamicProperties(o));

            if (IsJsonString(o) || IsJsonNumeric(o) || IsBool(o)) return Stringify(o);

            return Convert(ToHash(o));
        }

        private static IDictionary<string, object> PublicAndDynamicProperties(dynamic o)
        {
            Dictionary<string, object> properties = new Dictionary<string,object>();

            (o.Hash() as IDictionary<string, object>).ForEach(kvp => properties.Add(kvp.Key, kvp.Value));

            (o as object)
                .GetType()
                .GetProperties()
                .ForEach<PropertyInfo>(kvp => properties.Add(kvp.Name, kvp.GetValue(o, null)));

            return properties;
        }

        public static string Convert(IEnumerable<dynamic> o)
        {
            return "[ " + string.Join(", ", o.Select(s => Convert(s as object))) + " ]";
        }

        public static string Convert(IDictionary<string, object> attributes)
        {
            return "{ " + StringifyAttributes(attributes) + " }";
        }

        private static string StringifyAttributes(IDictionary<string, object> attributes)
        {
            return string.Join(", ", attributes.Where(CanConvertValue).Select(StringifyAttribute));
        }

        private static string StringifyAttribute(KeyValuePair<string, object> kvp)
        {
            return Stringify(kvp.Key) + ": " + Stringify(kvp.Value);
        }

        private static IDictionary<string, object> ToHash(object o)
        {
            var dictionary = (o as object).ToExpando() as IDictionary<string, object>;

            dictionary.Where(s => s.Value is IEnumerable<dynamic>).ForEach(s => dictionary[s.Key] = ToList(s.Value));

            return dictionary;
        }

        private static List<dynamic> ToList(dynamic enumerable)
        {
            return (enumerable as IEnumerable<dynamic>).ToList();
        }

        public static string Stringify(dynamic o)
        {
            if (IsJsonString(o)) return "\"" + o + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

            if (IsList(o)) return Convert(o as IEnumerable<dynamic>);

            if (IsBool(o)) return o.ToString().ToLower();

            if (o is Gemini) return Convert(o as object);

            return o.ToString();
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
            return IsJsonString(kvp.Value) || 
                   IsJsonNumeric(kvp.Value) || 
                   IsList(kvp.Value) || 
                   IsBool(kvp.Value) ||
                   kvp.Value is Gemini;
        }

        public static bool CanConvertObject(dynamic o)
        {
            if (o is ExpandoObject) return true;

            if (o is Gemini) return true;

            if (o is IEnumerable<object>) return (o as IEnumerable<object>).All(CanConvertObject);

            if (IsAnonymous(o)) return true;

            return false;
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