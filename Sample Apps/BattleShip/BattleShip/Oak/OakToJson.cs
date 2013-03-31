using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Dynamic;

namespace Oak
{
    public class DynamicToJson
    {
        public static string Convert(dynamic o)
        {
            if (o is IEnumerable<dynamic>) return Convert(o as IEnumerable<dynamic>);

            if(o is ExpandoObject) return Convert(o as IDictionary<string, object>);

            return Convert(o.Hash() as IDictionary<string, object>);
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
            return string.Join(", ", attributes.Where(CanConvert).Select(StringifyAttribute));
        }

        private static string StringifyAttribute(KeyValuePair<string, object> kvp)
        {
            return Stringify(kvp.Key) + ": " + Stringify(kvp.Value);
        }

        public static string Stringify(dynamic o)
        {
            if (IsJsonString(o)) return "\"" + o + "\"";

            if (IsJsonNumeric(o)) return o.ToString();

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
            return o.GetType() == typeof(Decimal) || o.GetType() == typeof(int) || o.GetType() == typeof(double);
        }

        public static bool CanConvert(KeyValuePair<string, object> kvp)
        {
            return IsJsonString(kvp.Value) || IsJsonNumeric(kvp.Value);
        }
    }
}