using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class InheritedGemini : Gemini
    {
        public InheritedGemini(object o)
            : base(o)
        {

        }

        public string FirstLetter()
        {
            return (Prototype.Title as string).First().ToString();
        }

        public string FirstName
        {
            get
            {
                return (Prototype.Title as string).Split(' ').First();
            }
        }
    }
}
