using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class presence : nspec
    {
        dynamic book;

        dynamic bookWithProps;

        bool isValid;

        void before_each()
        {
            book = new Book();

            bookWithProps = new BookWithProps();
        }

        void validating_precense_for_a_class_with_auto_property_defined()
        {
            act = () => isValid = bookWithProps.IsValid();

            context["both title and body are specified"] = () =>
            {
                before = () =>
                {
                    bookWithProps.Title = "Title";

                    bookWithProps.Body = "Body";
                };

                it["is valid"] = () => isValid.should_be_true();
            };
        }

        void validating_presense_of()
        {
            act = () => isValid = book.IsValid();

            context["title is empty"] = () => 
            {
                before = () => book.Title = string.Empty;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["body is populated, but title is empty"] = () =>
            {
                before = () => book.Body = "Body";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["title and body is populated"] = () =>
            {
                before = () =>
                {
                    book.Title = "Some Title";
                    book.Body = "Some Body";
                };

                it["is valid"] = () => isValid.should_be_true();
            };

            context["book is initialized with title and body are both set to default(string)"] = () =>
            {
                before = () =>
                {
                    book = new Book(new 
                    { 
                        Title = default(string), 
                        Body = default(string)
                    });
                };

                it["is invalid"] = () => isValid.should_be_false();
            };
        }
    }
}
