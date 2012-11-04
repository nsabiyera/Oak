using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_Gemini.Classes;

namespace Oak.Tests.describe_Gemini
{
    class implicitly_private_methods : nspec
    {
        public dynamic gemini;

        public class InnerGemini : Gemini
        {
            void InnerMethodThatThrows()
            {
                throw new InvalidOperationException("I threw an exception");
            }
        }

        public class TopLevelGemini : Gemini
        {
            dynamic gemini = new InnerGemini();

            void AMethod()
            {
                gemini.InnerMethodThatThrows();
            }
        }

        void specify_retaining_stack_trace_through_mulitple_private_calls()
        {
            dynamic toplevel = new TopLevelGemini();
            try
            {
                toplevel.AMethod();
            }
            catch (Exception ex)
            {
                ex.GetType().should_be(typeof(InvalidOperationException));

                ex.ToString().should_contain("AMethod");

                ex.ToString().should_contain("InnerMethodThatThrows");
            }
        }

        void private_dynamic_methods_and_functions()
        {
            act = () => gemini = new PrivateGemini();

            it["private function that take in no parameters and return dynamic are publicly accessible"] = () =>
                (gemini.HelloString() as string).should_be("hello");

            it["original exception is retained for private functions"] = expect<InvalidOperationException>(() => gemini.HelloException());

            it["private function that take in dynamic and return dynamic are publicly accessible"] = () =>
                (gemini.Hello("Jane") as string).should_be("hello Jane");

            it["private function that take in dynamic and return dynamic retain exception"] =
                expect<InvalidOperationException>(() => gemini.HelloExceptionWithParam("Jane"));

            it["private delegate that take in dynamic can interperet generic parameters"] = () =>
                (gemini.HelloFullName(firstName: "Jane", lastName: "Doe") as string).should_be("hello Jane Doe");

            it["private method that takes in no parameters is publically accessible"] = () =>
            {
                gemini.Alter();

                ((bool)gemini.Altered).should_be_true();
            };

            it["private method that takes in no parameters retains origin exception"] =
                expect<InvalidOperationException>(() => gemini.AlterException());

            it["private method that takes in dynamic parameter is publically accessible"] = () =>
            {
                gemini.SetAltered(true);

                ((bool)gemini.Altered).should_be_true();

                gemini.SetAltered(false);

                ((bool)gemini.Altered).should_be_false();
            };

            it["private method that takes in dynamic parameter retains exception"] =
                expect<InvalidOperationException>(() => gemini.SetAlteredException(true));

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
