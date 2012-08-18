using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class presence_for_static_type : presence
    {
        void before_each()
        {
            book = new BookWithProps();
        }
    }

    class presence_for_dynamic_type : presence
    {
        void before_each()
        {
            book = new Book();
        }
    }

    abstract class presence : nspec
    {
        public dynamic book;

        public bool isValid;

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
                    book.Id = 100;
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
