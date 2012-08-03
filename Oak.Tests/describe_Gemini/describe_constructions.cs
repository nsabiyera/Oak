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
    class describe_constructions : _describe_Gemini
    {
        void gemini_constructor_that_takes_in_dto()
        {
            before = () =>
            {
                blog = new ExpandoObject();
                blog.Title = "Working With Oak";
                blog.Body = "Oak is tight, yo.";
                blog.Author = "Amir";
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

                it["auto property is assigned as opposed to creating a new prop on the fly"] = () =>
                    (gemini.Author as string).should_be("Amir");
            };
        }

        void specify_initializing_gemini_with_null_does_not_throw_error()
        {
            gemini = new Gemini(null);
        }
    }
}
