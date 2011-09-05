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
            Attributes = new Dictionary<string, string>();
            Styles = new Dictionary<string, string>();
        }

        public object Value { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public Dictionary<string, string> Styles { get; set; }
    }
}