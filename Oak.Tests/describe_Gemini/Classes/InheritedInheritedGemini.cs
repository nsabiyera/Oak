using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class InheritedInheritedGemini : InheritedGemini
    {
        public InheritedInheritedGemini(object o)
            : base(o)
        {

        }

        public string LastLetter()
        {
            return (Expando.Title as string).Last().ToString();
        }
    }
}
