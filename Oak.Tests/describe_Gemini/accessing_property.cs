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

        void selecting_properties()
        {
            context["core behavior"] = () =>
            {
                before = () => gemini = new Gemini(new { Term = 10, Amount = 100000, Interest = .30, DueDate = DateTime.Today });

                act = () => gemini = gemini.Select("Term", "Amount");

                it["only responds to properties that were selected"] = () =>
                {
                    ((bool)gemini.RespondsTo("Term")).should_be(true);

                    ((bool)gemini.RespondsTo("Amount")).should_be(true);

                    ((bool)gemini.RespondsTo("Interest")).should_be(false);
                };
            };

            context["casing"] = () =>
            {
                before = () => gemini = new Gemini(new { Term = 10, Amount = 100000, Interest = .30, DueDate = DateTime.Today });

                act = () =>
                {
                    gemini = gemini.Select("term", "aMount", "dueDate");
                };

                it["case doesn't matter"] = () =>
                {
                    ((bool)gemini.RespondsTo("Term")).should_be(true);

                    ((bool)gemini.RespondsTo("Amount")).should_be(true);

                    ((bool)gemini.RespondsTo("DueDate")).should_be(true);
                };
            };
        }

        void excluding_properties()
        {
            context["core behavior"] = () =>
            {
                before = () => gemini = new Gemini(new { Term = 10, Amount = 100000, Interest = .30 });

                act = () => gemini = gemini.Exclude("Interest");

                it["only responds to properties that were selected"] = () =>
                {
                    ((bool)gemini.RespondsTo("Term")).should_be(true);

                    ((bool)gemini.RespondsTo("Amount")).should_be(true);

                    ((bool)gemini.RespondsTo("Interest")).should_be(false);
                };
            };

            context["casing", "wip"] = () =>
            {
                before = () => gemini = new Gemini(new { Term = 10, Amount = 100000, Interest = .30 });

                act = () => gemini = gemini.Exclude("inTerest");

                it["casing doesn't matter"] = () =>
                {
                    ((bool)gemini.RespondsTo("Amount")).should_be(true);
                    ((bool)gemini.RespondsTo("Interest")).should_be(false);
                };
            };
        }
    }
}
