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
    class inheritance_of_gemini : _describe_Gemini
    {
        dynamic inheritedGemini;

        void inherited_gemini_with_defined_methods()
        {
            act = () => inheritedGemini = new InheritedGemini(blog);

            it["calls underlying property"] = () =>
                (inheritedGemini.Title as string).should_be("Some Name");

            it["sets underlying property"] = () =>
            {
                inheritedGemini.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["calls defined method"] = () =>
                (inheritedGemini.FirstLetter() as string).should_be("S");

            it["calls defined property"] = () =>
                (inheritedGemini.FirstName as string).should_be("Some");
        }

        void double_inheritance()
        {
            act = () => inheritedGemini = new InheritedInheritedGemini(blog);

            it["calls methods on root gemini with"] = () =>
                (inheritedGemini.Title as string).should_be("Some Name");

            it["calls method on first gemini"] = () =>
                (inheritedGemini.FirstLetter() as string).should_be("S");

            it["calls method on top most gemini"] = () =>
                (inheritedGemini.LastLetter() as string).should_be("e");
        }
    }
}
