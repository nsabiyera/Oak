using System;
using System.Linq;
using System.Collections.Generic;

namespace RestfulSilverlight
{
    public static class DictionaryExtensions
    {
        public static Dictionary<string, object> ToDictionary(this object value)
        {
            return value as Dictionary<string, object>;
        }

        public static Guid Guid(this object value)
        {
            return new Guid(value.ToString());
        }

        public static decimal Decimal(this object value)
        {
            return Convert.ToDecimal(value);
        }

        public static DateTime Date(this object value)
        {
            return Convert.ToDateTime(value.ToString());
        }

        public static string String(this object value)
        {
            return value.ToString();
        }

        public static Hashes ToHashes(this object value)
        {
            Hashes array = new Hashes();
            var objectArray = value as Object[];
            foreach (Object current in objectArray)
            {
                array.Add(current as Hash);
            }

            return array;
        }

        public static string Href(this object value, string rel)
        {
            var hashes = value as Hashes;

            if (hashes == null)
            {
                hashes = value.ToHashes();
            }

            return hashes.Single(s => s["Rel"].ToString() == rel)["Href"].ToString();
        }

        public static bool HasRel(this object value, string rel)
        {
            var hashes = value as Hashes;

            if (hashes == null)
            {
                hashes = value.ToHashes();
            }

            return hashes.Any(s => s["Rel"].ToString() == rel);
        }
    }
}
