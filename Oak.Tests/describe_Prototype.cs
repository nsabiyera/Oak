using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;

namespace Oak.Tests
{
    class describe_Prototype : nspec
    {
        dynamic blog;

        dynamic prototype;

        dynamic inheritedPrototype;

        void before_each()
        {
            blog = new ExpandoObject();
            blog.Title = "Some Name";
            blog.body = "Some Body";
            blog.BodySummary = "Body Summary";
            prototype = new Prototype(blog);
        }

        void describe_responds_to()
        {
            it["responds to property with exact casing"] = () => Prototype().RespondsTo("Title").should_be_true();

            it["it responds to property with case insensitive"] = () => Prototype().RespondsTo("title").should_be_true();

            it["it doesn't respond to property"] = () => Prototype().RespondsTo("foobar").should_be_false();
        }

        void describe_methods()
        {
            it["it contains a record for each method defined"] = () =>
            {
                Prototype().Methods().should_contain("Title");
                Prototype().Methods().should_contain("body");
                Prototype().Methods().should_contain("BodySummary");
            };
        }

        void deleting_members()
        {
            context["given a member is defined"] = () =>
            {
                before = () => Prototype().RespondsTo("Title").should_be_true();

                act = () => Prototype().DeleteMember("Title");

                it["no longer responds to member"] = () => Prototype().RespondsTo("Title").should_be_false();
            };

            new[] { "title", "TITLE" }.Do(member =>
            {
                context["member deletion is case insensitive ({0})".With(member)] = () =>
                {
                    before = () => Prototype().RespondsTo("Title").should_be_true();

                    act = () => Prototype().DeleteMember(member);

                    it["no longer responds to member"] = () => Prototype().RespondsTo("Title").should_be_false();
                };
            });
            

            context["member is not defined"] = () =>
            {
                before = () => Prototype().RespondsTo("FooBar").should_be_false();

                act = () => Prototype().DeleteMember("FooBar");

                it["ignores deletion"] = () => Prototype().RespondsTo("FooBar").should_be_false();
            };

            
        }

        void describe_get_value_for_property()
        {
            it["retrieves value with exact casing"] = () => (Prototype().GetValueFor("Title") as string).should_be("Some Name");

            it["retrieves value with exact case insensitive"] = () => (Prototype().GetValueFor("title") as string).should_be("Some Name");

            it["throws invalid op if property doesn't exist"] = expect<InvalidOperationException>("This prototype does not respond to the property FooBar.", () => Prototype().GetValueFor("FooBar"));
        }

        void when_retrieving_property_from_prototype()
        {
            it["calls values for prototypeed entity"] = () =>
                (prototype.Title as string).should_be("Some Name");

            it["calls value for prototypeed entity even if property's first letter doesn't match case"] = () =>
                (prototype.title as string).should_be("Some Name");

            it["calls value for prototypeed entity even if property's first letter is capilized, but underlying property is lowercase"] = () =>
                (prototype.Body as string).should_be("Some Body");

            it["ignores case for prototypeed entity"] = () =>
                (prototype.bodysummary as string).should_be("Body Summary");
        }

        void when_setting_property_of_prototype()
        {
            it["sets property on underlying expando"] = () =>
            {
                prototype.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter doesn't match case"] = () =>
            {
                prototype.title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying expando even if property's first letter is capitalized, but underlying property is lowercase"] = () =>
            {
                prototype.Body = "Some other name";
                (blog.body as string).should_be("Some other name");
            };

            it["ignores case for prototypeed entity"] = () =>
            {
                prototype.bodysummary = "Blog Summary New";
                (blog.BodySummary as string).should_be("Blog Summary New");
            };

            it["sets property to a new value if the property doesn't exist"] = () =>
            {
                prototype.FooBar = "Foobar";

                (blog.FooBar as string).should_be("Foobar");
            };
        }

        void inherited_prototypeed_with_defined_methods()
        {
            act = () => inheritedPrototype = new InheritedPrototype(blog);

            it["calls underlying property"] = () =>
                (inheritedPrototype.Title as string).should_be("Some Name");

            it["sets underlying property"] = () =>
            {
                inheritedPrototype.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["calls defined method"] = () =>
                (inheritedPrototype.FirstLetter() as string).should_be("S");

            it["calls defined property"] = () =>
                (inheritedPrototype.FirstName as string).should_be("Some");
        }

        void double_inheritance()
        {
            act = () => inheritedPrototype = new InheritedInheritedPrototype(blog);

            it["calls methods on root prototype with"] = () =>
                (inheritedPrototype.Title as string).should_be("Some Name");

            it["calls method on first prototype"] = () =>
                (inheritedPrototype.FirstLetter() as string).should_be("S");

            it["calls method on top most prototype"] = () =>
                (inheritedPrototype.LastLetter() as string).should_be("e");
        }

        void given_a_blog()
        {
            before = () =>
            {
                blog = new ExpandoObject();
                blog.Title = "Working With Oak";
                blog.Body = "Oak is tight, yo.";
            };

            context["given that the dynamic blogged is wrapped with a prototype"] = () =>
            {
                before = () =>
                    prototype = new BlogEntry(blog);

                it["base properties are still accessible"] = () =>
                    (prototype.Title as string).should_be("Working With Oak");

                it["base properties are still settable"] = () =>
                {
                    prototype.Title = "Another Title";
                    (blog.Title as string).should_be("Another Title");
                };

                it["new properites provided by BlogEntry prototype are available"] = () =>
                    ((bool)prototype.IsValid()).should_be_true();

                it["properites defined in the prototype do override base properties"] = () =>
                    (prototype.Body as string).should_be("");
            };

        }

        void working_with_parameterless_prototype_that_defines_properites_in_the_constructor()
        {
            before = () => prototype = new ParameterlessPrototype();

            it["properties are accessible"] = () => (prototype.FirstName as string).should_be("");

            context["tacking on properties after the fact is allowed"] = () =>
            {
                act = () => prototype.Expando.NewProp = "new prop";

                it["new prop is accessible"] = () => (prototype.NewProp as string).should_be("new prop");
            };

            context["tacking on methods after the fact is allowed"] = () =>
            {
                act = () => prototype.Expando.NewProp = new Func<string, string>((s) => s.ToUpper());

                it["new method is accessible"] = () => (prototype.NewProp("hello") as string).should_be("HELLO");
            };
        }

        Prototype Prototype()
        {
            return prototype as Prototype;
        }
    }

    public class BlogEntry : Prototype
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

    public class InheritedInheritedPrototype : InheritedPrototype
    {
        public InheritedInheritedPrototype(object o)
            : base(o)
    	{
    				
    	}

        public string LastLetter()
        {
            return (Expando.Title as string).Last().ToString();
        }
    }

    public class InheritedPrototype : Prototype
    {
        public InheritedPrototype(object o)
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

    public class ParameterlessPrototype : Prototype
    {
        public ParameterlessPrototype()
        {
            Expando.FirstName = "";
            Expando.LastName = "";
        }
    }
}
