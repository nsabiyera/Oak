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

        void inherited_gemini_with_private_dynamic_methods_and_functions()
        {
            act = () => inheritedGemini = new PrivateGemini();

            it["private function that take in no parameters and return dynamic are publicly accessible"] = () =>
                (inheritedGemini.HelloString() as string).should_be("hello");

            it["private function that take in dynamic and return dynamic are publicly accessible"] = () =>
                (inheritedGemini.Hello("Jane") as string).should_be("hello Jane");

            it["private delegate that take in dynamic can interperet generic parameters"] = () =>
                (inheritedGemini.HelloFullName(firstName: "Jane", lastName: "Doe") as string).should_be("hello Jane Doe");

            it["private method that takes in no parameters is publically accessible"] = () =>
            {
                inheritedGemini.Alter();

                ((bool)inheritedGemini.Altered).should_be_true();
            };

            it["private method that takes in dynamic parameter is publically accessible"] = () =>
            {
                inheritedGemini.SetAltered(true);

                ((bool)inheritedGemini.Altered).should_be_true();

                inheritedGemini.SetAltered(false);

                ((bool)inheritedGemini.Altered).should_be_false();
            };

            it["private function that return enumerable of dynamic is publically accessible"] = () =>
                (inheritedGemini.Names() as IEnumerable<dynamic>).should_contain("name1");

            it["private function that takes a parameter and returns enumerable of dynamic is publically accessible"] = () =>
                (inheritedGemini.NamesWithPrefix("hi") as IEnumerable<dynamic>).should_contain("hiname1");

            it["private delegate (returning enumerable) that take in dynamic can interperet generic parameters"] = () =>
                (inheritedGemini.NamesWithArgs(prefix: "hi") as IEnumerable<dynamic>).should_contain("hiname1");

            it["private members can be redefined"] = () =>
            {
                (inheritedGemini.HelloString() as string).should_be("hello");

                inheritedGemini.HelloString = "booya";

                (inheritedGemini.HelloString as string).should_be("booya");
            };
        }
    }
}
