using System;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Length
{
    class length : nspec
    {
        dynamic essay;

        bool isValid;

        void before_each()
        {
            essay = new Essay();
            essay.Title = "valid title";
            essay.Author = "bob";
            essay.Publisher = "WorldWide";
            essay.Version = "1";
            essay.Year = "1990";
        }
        
        void validating_length_of()
        {
            act = () => isValid = essay.IsValid();

            context["title is not long enough"] = () =>
            {
                before = () => essay.Title = string.Empty;
                
                it["is not valid"] = () => isValid.should_be_false();
            };

            context["title is exact length"] = () =>
            {
                before = () => essay.Title = "1";
                
                it["is valid"] = () => isValid.should_be_true();
            };

            context["title is greater than length"] = () =>
            {
                before = () => essay.Title = "12345";
                
                it["is valid"] = () => isValid.should_be_true();
            };

            context["author is greater than length"] = () =>
            {
                before = () => essay.Author = "123456";
                
                it["is not valid"] = () => isValid.should_be_false();
            };

            context["author is exact length"] = () =>
            {
                before = () => essay.Author = "12345";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["author is less than length"] = () =>
            {
                before = () => essay.Author = "1";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["publish not in range"] = () =>
            {
                before = () => essay.Publisher = string.Empty;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["publish is minimum in range"] = () =>
            {
                before = () => essay.Publisher = "1";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["publish is in range"] = () =>
            {
                before = () => essay.Publisher = "123";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["publish is maximum in range"] = () =>
            {
                before = () => essay.Publisher = "1234567890";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["publish is above range"] = () =>
            {
                before = () => essay.Publisher = "12345678901";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["version is below value"] = () =>
            {
                before = () => essay.Version = string.Empty;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["version is above value"] = () =>
            {
                before = () => essay.Version = "12";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["version is equal to value"] = () =>
            {
                before = () => essay.Version = "1";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["property does not contain a length property"] = () =>
            {
                before = () => essay.Year = 1990;

                it["should throw exception"] = expect<Exception>(() => string.Empty.Trim());
            };
        }
    }
}
