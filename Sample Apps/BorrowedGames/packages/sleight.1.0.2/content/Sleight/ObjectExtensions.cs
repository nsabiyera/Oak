using System;

namespace Sleight
{
    public static class ObjectExtensions
    {
        public static dynamic ValueOrNull(this object dictionary, dynamic key)
        {
            dynamic temp = dictionary;

            if (temp.ContainsKey(key)) return temp[key];

            return null;
        }
    }
}
