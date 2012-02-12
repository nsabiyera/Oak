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
    class describe_responds_to : _describe_Gemini
    {
        void it_responds_to_property_with_exact_casing()
        {
            Gemini().RespondsTo("Title").should_be_true();
        }

        void it_responds_to_property_with_case_insensitive()
        {
            Gemini().RespondsTo("title").should_be_true();
        }

        void it_doesnt_respond_to_property()
        {
            Gemini().RespondsTo("foobar").should_be_false();
        }
    }
}
