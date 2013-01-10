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
    class setting_member : _describe_Gemini
    {
        void when_setting_property_of_gemini()
        {
            it["sets property on underlying prototype"] = () =>
            {
                gemini.Title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying prototype even if property's first letter doesn't match case"] = () =>
            {
                gemini.title = "Some other name";
                (blog.Title as string).should_be("Some other name");
            };

            it["sets property of underlying prototype even if property's first letter is capitalized, but underlying property is lowercase"] = () =>
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

            it["reference to self are added as methods"] = () =>
            {
                dynamic self = new Gemini(new { FirstName = "Amir", LastName = "Rajan" });

                self.Self = self;

                (self.Self() as object).should_be(self as object);
            };

            it["references to self where prototypes match are added as methods"] = () =>
            {
                dynamic self = new Gemini(new { FirstName = "Amir", LastName = "Rajan" });

                dynamic samePrototype = new Gemini(self);

                self.Self = samePrototype;

                (self.Self() as object).should_be(samePrototype as object);
            };
        }

        void calling_set_members()
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
    }
}
