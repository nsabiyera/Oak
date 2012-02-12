using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;
using Oak.Tests.describe_Gemini.Classes;

namespace Oak.Tests.describe_Gemini
{
    class describe_methods : _describe_Gemini
    {
        void it_contains_a_record_for_each_method_defined()
        {
            Gemini().Members().should_contain("Title");
            Gemini().Members().should_contain("body");
            Gemini().Members().should_contain("BodySummary");
        }
    }
}
