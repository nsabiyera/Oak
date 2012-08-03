using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class inclusion_for_type_with_auto_props : inclusion
    {
        void before_each()
        {
            coffee = new CoffeeWithAutoProperties();
        }
    }

    class inclusion_for_dynamic_type : inclusion
    {
        void before_each()
        {
            coffee = new Coffee();
        }
    }

    abstract class inclusion : nspec
    {
        public dynamic coffee;

        public bool isValid;

        void validating_inclusion()
        {
            act = () => isValid = coffee.IsValid();

            context["coffee size is not a valid size"] = () =>
            {
                before = () => coffee.Size = "tall";

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["coffee is valid size"] = () =>
            {
                before = () => coffee.Size = "small";

                it["is valid"] = () => isValid.should_be_true();
            };
        }
    }
}
