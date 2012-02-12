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
    class accessing_property : _describe_Gemini
    {
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

        void properites_defined_on_underlying_expando()
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
    }
}
