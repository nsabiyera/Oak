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

        void properites_defined_on_underlying_prototype()
        {
            before = () => gemini = new ParameterlessGemini();

            it["properties are accessible"] = () => (gemini.FirstName as string).should_be("");

            context["tacking on properties after the fact is allowed"] = () =>
            {
                act = () => gemini.Prototype.NewProp = "new prop";

                it["new prop is accessible"] = () => (gemini.NewProp as string).should_be("new prop");
            };

            context["tacking on methods after the fact is allowed"] = () =>
            {
                act = () => gemini.Prototype.NewProp = new Func<string, string>((s) => s.ToUpper());

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

            context["collections"] = () =>
            {
                before = () => gemini = new Gemini(new List<string> { "First", "Second" });

                it["initializes a property called items and sets it to the collection"] = () =>
                {
                    (gemini.Items as List<string>).Count().should_be(2);

                    (gemini.Items[0] as string).should_be("First");

                    (gemini.Items[1] as string).should_be("Second");
                };
            };

            context["strings"] = () =>
            {
                before = () => gemini = new Gemini("hello");

                it["initializes a property called value"] = () =>
                    (gemini.Value as object).should_be("hello");
            };

            context["value types"] = () =>
            {
                before = () => gemini = new Gemini(15);

                it["initializes a property called value"] = () =>
                    (gemini.Value as object).should_be(15);
            };

            context["expando object"] = () =>
            {
                before = () =>
                {
                    dynamic expando = new ExpandoObject();
                    expando.FirstName = "First";
                    expando.LastName = "Last";
                    gemini = new Gemini(expando);
                };

                it["doesn't consider an expando object as enumerable"] = () =>
                {
                    (gemini.FirstName as object).should_be("First");

                    (gemini.LastName as object).should_be("Last");
                };
            };

            context["string dictionaries"] = () =>
            {
                before = () =>
                {
                    gemini = new Gemini(new Dictionary<string, object> 
                    { 
                        { "FirstName", "First" },
                        { "LastName", "Last" }
                    });
                };

                it["doesn't consider a IDictionary<string, object> as enumerable"] = () =>
                {
                    (gemini.FirstName as object).should_be("First");

                    (gemini.LastName as object).should_be("Last");
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

            context["casing"] = () =>
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
