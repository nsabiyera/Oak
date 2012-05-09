using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_Gemini.Classes;

namespace Oak.Tests.describe_Gemini
{
    [Tag("wip")]
    class describe_types : nspec
    {
        void specify_of_type_for_gemini_compares_direct_type()
        {
            dynamic blog = new BlogEntry(new { });

            ((bool)blog.IsOfType<BlogEntry>()).should_be_true();
        }

        void specify_of_type_for_object_compares_direct_type()
        {
            var s = "foobar";

            s.IsOfType<string>().should_be_true();
        }

        void of_kind()
        {
            it["looks at inheritance heirarchy for strongly typed objects"] = () =>
            {
                "foobar".IsOfKind<string>().should_be_true();

                "foobar".IsOfKind<object>().should_be_true();
            };

            it["looks at inheritance heirarchy for gemini's"] = () =>
            {
                dynamic blog = new BlogEntry(new { });

                ((bool)blog.IsOfKind<BlogEntry>()).should_be_true();

                ((bool)blog.IsOfKind<Gemini>()).should_be_true();
            };

            it["includes modules that are mixed in for a gemini instance"] = () =>
            {
                dynamic blog = new BlogEntry(new { });

                blog.Extend<Changes>();

                ((bool)blog.IsOfKind<Changes>()).should_be_true();
            };
        }
    }
}
