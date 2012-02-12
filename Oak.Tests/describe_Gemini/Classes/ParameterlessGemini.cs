using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class ParameterlessGemini : Gemini
    {
        public ParameterlessGemini()
        {
            Expando.FirstName = "";
            Expando.LastName = "";
        }
    }
}
