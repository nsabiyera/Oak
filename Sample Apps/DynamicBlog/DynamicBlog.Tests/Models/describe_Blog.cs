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
        bool isValid;

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

        void blog_validation()
        {
            act = () => isValid = blog.IsValid();

            context["title and body are provided"] = () =>
            {
                before = () =>
                {
                    blog.Title = "Title";
                    blog.Body = "Body";
                };

                it["is valid"] = () => isValid.should_be_true();
            };

            context["only title is provided"] = () =>
            {
                before = () =>
                {
                    blog.Title = "Title";
                    blog.Body = string.Empty;
                };

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["only body is provided"] = () =>
            {
                before = () =>
                {
                    blog.Title = string.Empty;
                    blog.Body = "Body";
                };

                it["is invalid"] = () => isValid.should_be_false();
            };
        }

        void GivenBlog(string withBody)
        {
            blog = new Blog(new { Body = withBody });
        }
    }
}
