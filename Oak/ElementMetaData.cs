using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Oak
{
    public class ElementMetaData
    {
        public object Value { get; set; }
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public Dictionary<string, string> Styles { get; set; }
    }
}