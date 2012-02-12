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
