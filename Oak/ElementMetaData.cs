using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oak
{
    public class ElementMetaData
    {
        string value;
        string id;

        public ElementMetaData()
        {
            Hash = new Hash();
            value = null;
            id = null;
        }

        public string Value()
        {
            return value;
        }

        public string Id()
        {
            return id;
        }

        public void Set(string key, string value)
        {
            if (key == "value" && this.value == null)
            {
                this.value = value;
                return;
            }

            if(key == "id")
            {
                id = value;
                return;
            }

            if (!Hash.ContainsKey(key)) Hash.Add(key, null);

            Hash[key] = value;
        }

        public Hash Hash { get; set; }
    }
}