using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.SampleClasses;

namespace Oak.Tests.describe_DynamicModel
{
    class core_validation : nspec
    {
        dynamic book;

        string firstError;

        bool isValid;

        void before_each()
        {
            book = new Book();

            book.Title = "Title";

            book.Body = "Body";
        }

        void act_each()
        {
            book.IsValid();
        }

        void describe_first_error()
        {
            act = () => firstError = book.FirstError();

            context["both title and body are not specified in Book"] = () =>
            {
                before = () =>
                {
                    book.Title = string.Empty;
                    book.Body = string.Empty;
                };

                it["contains 2 errors"] = () => ((int)book.Errors().Count).should_be(2);

                it["first error should be Title is required"] = () => firstError.should_be("Title is required.");
            };

            context["title is specified but body is not"] = () =>
            {
                before = () =>
                {
                    book.Title = "Title";
                    book.Body = string.Empty;
                };

                it["contains 1 error"] = () => ((int)book.Errors().Count).should_be(1);

                it["first error should be Body is required"] = () => firstError.should_be("Body is required.");
            };
        }

        void case_insensitive_valdation()
        {
            act = () => isValid = book.IsValid();

            context["setting the title with a lower case T"] = () =>
            {
                before = () =>
                {
                    book = new Book(new { title = "Title", body = "Body" });
                };

                it["is still valid"] = () => isValid.should_be_true();
            };
        }
    }
}
