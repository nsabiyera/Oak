using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Oak;

namespace RestfulClient
{
    public class JsonString
    {
        public static string Parse(dynamic o)
        {
            if(o is Gemini) return (o as Gemini).ToString();

            return "I can only parse objects of type Gemini";
        }
    }
}
