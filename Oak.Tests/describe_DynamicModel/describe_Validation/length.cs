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
            essay.Title = string.Empty;
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
        }
    }
}
