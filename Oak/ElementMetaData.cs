using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oak
{
    public class ElementMetaData
    {
        public ElementMetaData()
        {
            Hash = new Hash();
        }

        public string Value()
        {
            return Hash["value"];
        }

        public string Id()
        {
            return Hash["id"];
        }

        public void Set(string key, string value)
        {
            if (!Hash.ContainsKey(key)) Hash.Add(key, null);

            if (key == "value" && Hash[key] != null) return;

            Hash[key] = value;
        }

        public Hash Hash { get; set; }
    }
}