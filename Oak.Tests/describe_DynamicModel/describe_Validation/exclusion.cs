using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class exclusion : nspec
    {
        dynamic registration;

        bool isValid;

        void before_each()
        {
            registration = new Registration();

            registration.UserName = "user";
        }

        void validating_exclusions()
        {
            act = () => isValid = registration.IsValid();

            context["UserName matches the user name 'admin'"] = () =>
            {
                before = () => registration.UserName = "admin";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["UserName matches the user name 'administrator'"] = () =>
            {
                before = () => registration.UserName = "administrator";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["UserName does not match an entry in the exclusion list"] = () =>
            {
                before = () => registration.UserName = "user";

                it["is valid"] = () => isValid.should_be_true();
            };
        }
    }
}
