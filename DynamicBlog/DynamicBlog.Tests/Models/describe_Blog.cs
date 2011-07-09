using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using DynamicBlog.Models;

namespace DynamicBlog.Tests.Models
{
    class describe_Blog : nspec
    {
        dynamic blog;
        string summary;

        void describe_summary()
        {
            act = () => summary = blog.Summary;

            context["body is null"] = () =>
            {
                before = () => GivenBlog(withBody: null);

                it["summary returns empty string"] = () => summary.should_be("");
            };

            context["body is under 50 characters"] = () =>
            {
                before = () => GivenBlog(withBody: "Blog under 50 characters");

                it["returns body verbatim"] = () => summary.should_be("Blog under 50 characters");
            };

            context["body over 50 characters"] = () =>
            {
                before = () => GivenBlog(withBody: "01234567890123456789012345678901234567890123456789OVER50");

                it["returns the first 50 characters"] = () => summary.should_be("01234567890123456789012345678901234567890123456789");
            };
        }

        void GivenBlog(string withBody)
        {
            blog = new Blog(new { Body = withBody });
        }
    }
}
