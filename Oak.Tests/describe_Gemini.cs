using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;

namespace Oak.Tests
{
    class describe_Gemini : nspec
    {
        dynamic blog;

        dynamic gemini;

        dynamic inheritedGemini;

        void before_each()
        {
            blog = new ExpandoObject();
            blog.Title = "Some Name";
            blog.body = "Some Body";
            blog.BodySummary = "Body Summary";
            gemini = new Gemini(blog);
        }

        void describe_responds_to()
        {
            it["responds to property with exact casing"] = () => Gemini().RespondsTo("Title").should_be_true();

            it["it responds to property with case insensitive"] = () => Gemini().RespondsTo("title").should_be_true();

            it["it doesn't respond to property"] = () => Gemini().RespondsTo("foobar").should_be_false();
        }

        void describe_methods()
        {
            it["it contains a record for each method defined"] = () =>
            {
                Gemini().Members().should_contain("Title");
                Gemini().Members().should_contain("body");
                Gemini().Members().should_contain("BodySummary");
            };
        }

        void deleting_members()
        {
            context["given a member is defined"] = () =>
            {
                before = () => Gemini().RespondsTo("Title").should_be_true();

                act = () => Gemini().DeleteMember("Title");

                it["no longer responds to member"] = () => Gemini().RespondsTo("Title").should_be_false();
            };

            new[] { "title", "TITLE" }.Do(member =>
            {
                context["member deletion is case insensitive ({0})".With(member)] = () =>
                {
                    before = () => Gemini().RespondsTo("Title").should_be_true();

                    act = () => Gemini().DeleteMember(member);

                    it["no longer responds to member"] = () => Gemini().RespondsTo("Title").should_be_false();
                };
            });
            

            context["member is not defined"] = () =>
            {
                before = () => Gemini().RespondsTo("FooBar").should_be_false();

                act = () => Gemini().DeleteMember("FooBar");

                it["ignores deletion"] = () => Gemini().RespondsTo("FooBar").should_be_false();
            };
        }

        void describe_get_value_for_property()
        {
            it["retrieves value with exact casing"] = () => 
                (Gemini().GetMember("Title") as string).should_be("Some Name");

            it["retrieves value with exact case insensitive"] = () => 
                (Gemini().GetMember("title") as string).should_be("Some Name");

            it["throws invalid op if property doesn't exist"] = 
                expect<InvalidOperationException>("This instance of type Gemini does not respond to the property FooBar.  These are the members that exist on this instance: Title (String), body (String), BodySummary (String)", () => Gemini().GetMember("FooBar"));
        }

        void when_retrieving_property_from_gemini()
        {
            it["calls values for geminied entity"] = () =>
                (gemini.Title as string).should_be("Some Name");

            it["calls value for geminied entity even if property's first letter doesn't match case"] = () =>
                (gemini.title as string).should_be("Some Name");

            it["calls value for geminied entity even if property's first letter is capilized, but underlying property is lowercase"] = () =>
                (gemini.Body as string).should_be("Some Body");

            it["ignores case for geminied entity"] = () =>
                (gemini.bodysummary as string).should_be("Body Summary");
        }

        void when_setting_property_of_gemini()
        {
            it["sets property on underlying expando"] = () =>
            {
                gemini.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter doesn't match case"] = () =>
            {
                gemini.title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter is capitalized, but underlying property is lowercase"] = () =>
            {
                gemini.Body = "Some other name";
                (blog.body as string).should_be("Some other name");
            };

            it["ignores case for geminied entity"] = () =>
            {
                gemini.bodysummary = "Blog Summary New";
                (blog.BodySummary as string).should_be("Blog Summary New");
            };

            it["sets property to a new value if the property doesn't exist"] = () =>
            {
                gemini.FooBar = "Foobar";

                (blog.FooBar as string).should_be("Foobar");
            };
        }

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

        void given_a_blog()
        {
            before = () =>
            {
                blog = new ExpandoObject();
                blog.Title = "Working With Oak";
                blog.Body = "Oak is tight, yo.";
            };

            context["given that the dynamic blogged is wrapped with a gemini"] = () =>
            {
                before = () =>
                    gemini = new BlogEntry(blog);

                it["base properties are still accessible"] = () =>
                    (gemini.Title as string).should_be("Working With Oak");

                it["base properties are still settable"] = () =>
                {
                    gemini.Title = "Another Title";
                    (blog.Title as string).should_be("Another Title");
                };

                it["new properites provided by BlogEntry gemini are available"] = () =>
                    ((bool)gemini.IsValid()).should_be_true();

                it["properites defined in the gemini override base properties"] = () =>
                    (gemini.Body as string).should_be("");
            };
        }

        void specify_initializing_gemini_with_null_does_not_throw_error()
        {
            gemini = new Gemini(null);
        }

        void working_with_parameterless_gemini_that_defines_properites_in_the_constructor()
        {
            before = () => gemini = new ParameterlessGemini();

            it["properties are accessible"] = () => (gemini.FirstName as string).should_be("");

            context["tacking on properties after the fact is allowed"] = () =>
            {
                act = () => gemini.Expando.NewProp = "new prop";

                it["new prop is accessible"] = () => (gemini.NewProp as string).should_be("new prop");
            };

            context["tacking on methods after the fact is allowed"] = () =>
            {
                act = () => gemini.Expando.NewProp = new Func<string, string>((s) => s.ToUpper());

                it["new method is accessible"] = () => (gemini.NewProp("hello") as string).should_be("HELLO");
            };
        }

        void setting_members()
        {
            context["setting a single member"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.SetMember("FirstName", "Amir");
                };

                it["sets member"] = () => (gemini.FirstName as string).should_be("Amir");
            };

            context["setting multiple members"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.SetMembers(new { FirstName = "Amir", LastName = "Rajan" });
                };

                it["sets multiple members"] = () =>
                {
                    (gemini.FirstName as string).should_be("Amir");
                    (gemini.LastName as string).should_be("Rajan");
                };
            };
        }

        void calling_dynamically_defined_methods()
        {
            context["method is defined as a dynamic function that takes in one dynamic parameter"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.CreateNewGemini = new DynamicFunctionWithParam(d => new Gemini(d));
                };

                it["calls method with parameter specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini(new { FirstName = "Amir" });

                    (newGemini.FirstName as string).should_be("Amir");
                };

                it["calls method with even if parameter is not specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini();

                    (newGemini as object).should_not_be_null();
                };

                it["calls method with parameters specified as named parameters"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini(FirstName: "Amir");

                    (newGemini.FirstName as string).should_be("Amir");
                };

                it["ignores unnamed parameters if name parameters have been specified"] = () =>
                {
                    dynamic newGemini = gemini.CreateNewGemini("Unnamed", FirstName: "Amir", LastName: "Rajan");

                    (newGemini.FirstName as string).should_be("Amir");

                    (newGemini.LastName as string).should_be("Rajan");
                };
            };

            context["method is defined as a dynamic method that takes in one dynamic parameter"] = () =>
            {
                act = () =>
                {
                    gemini = new ParameterlessGemini();
                    gemini.AlterGemini = new DynamicMethodWithParam(d => gemini.Property = d ?? "Default");
                };

                it["calls method with parameter specified"] = () =>
                {
                    gemini.AlterGemini("Other");

                    (gemini.Property as string).should_be("Other");
                };

                it["calls method with even if parameter is not specified"] = () =>
                {
                    gemini.AlterGemini();

                    (gemini.Property as string).should_be("Default");
                };
            };
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

        Gemini Gemini()
        {
            return gemini as Gemini;
        }
    }

    public class BlogEntry : Gemini
    {
        public BlogEntry(object o)
            : base(o)
        {

        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Expando.Title);
        }

        public string Body
        {
            get
            {
                return "";
            }
        }
    }

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

    public class InheritedGemini : Gemini
    {
        public InheritedGemini(object o)
            : base(o)
        {

        }

        public string FirstLetter()
        {
            return (Expando.Title as string).First().ToString();
        }

        public string FirstName
        {
            get
            {
                return (Expando.Title as string).Split(' ').First();
            }
        }
    }

    public class ParameterlessGemini : Gemini
    {
        public ParameterlessGemini()
        {
            Expando.FirstName = "";
            Expando.LastName = "";
        }
    }

    public class PrivateGemini  : Gemini
    {
        public bool Altered;

        dynamic HelloString()
        {
            return "hello";
        }

        dynamic Hello(dynamic name)
        {
            return "hello " + name;
        }

        dynamic HelloFullName(dynamic fullName)
        {
            return "hello " + fullName.firstName + " " + fullName.lastName;
        }

        void Alter()
        {
            Altered = true;
        }

        void SetAltered(dynamic value)
        {
            Altered = value;
        }

        IEnumerable<dynamic> Names()
        {
            return new[] { "name1", "name2" };
        }

        IEnumerable<dynamic> NamesWithPrefix(dynamic prefix)
        {
            return Names().Select(s => prefix + s);
        }

        IEnumerable<dynamic> NamesWithArgs(dynamic args)
        {
            return Names().Select(s => args.prefix + s);
        }
    }
}
