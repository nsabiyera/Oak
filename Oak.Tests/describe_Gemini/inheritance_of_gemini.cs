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

        void private_dynamic_methods_and_functions()
        {
            act = () => gemini = new PrivateGemini();

            it["private function that take in no parameters and return dynamic are publicly accessible"] = () =>
                (gemini.HelloString() as string).should_be("hello");

            it["private function that take in dynamic and return dynamic are publicly accessible"] = () =>
                (gemini.Hello("Jane") as string).should_be("hello Jane");

            it["private delegate that take in dynamic can interperet generic parameters"] = () =>
                (gemini.HelloFullName(firstName: "Jane", lastName: "Doe") as string).should_be("hello Jane Doe");

            it["private method that takes in no parameters is publically accessible"] = () =>
            {
                gemini.Alter();

                ((bool)gemini.Altered).should_be_true();
            };

            it["private method that takes in dynamic parameter is publically accessible"] = () =>
            {
                gemini.SetAltered(true);

                ((bool)gemini.Altered).should_be_true();

                gemini.SetAltered(false);

                ((bool)gemini.Altered).should_be_false();
            };

            it["private function that return enumerable of dynamic is publically accessible"] = () =>
                (gemini.Names() as IEnumerable<dynamic>).should_contain("name1");

            it["private function that takes a parameter and returns enumerable of dynamic is publically accessible"] = () =>
                (gemini.NamesWithPrefix("hi") as IEnumerable<dynamic>).should_contain("hiname1");

            it["private delegate (returning enumerable) that take in dynamic can interperet generic parameters"] = () =>
                (gemini.NamesWithArgs(prefix: "hi") as IEnumerable<dynamic>).should_contain("hiname1");

            it["private members can be redefined"] = () =>
            {
                (gemini.HelloString() as string).should_be("hello");

                gemini.HelloString = "booya";

                (gemini.HelloString as string).should_be("booya");
            };
        }

        void inheriting_gemini_that_has_private_methods()
        {
            act = () => gemini = new InheritedPrivateGemini();

            it["methods defined on base class are publically defined on inherited class"] = () =>
                (gemini.HelloString() as string).should_be("hello");
        }

        void double_inherited_gemini_that_has_private_methods()
        {
            act = () => gemini = new DoubleInheritedPrivateGemini();

            it["methods defined on base class are publically defined on inherited class"] = () =>
                (gemini.HelloString() as string).should_be("hello");
        }
    }
}
