using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class acceptance : nspec
    {
        dynamic legalDocument;

        bool isValid;

        string error;

        void before_each()
        {
            legalDocument = new LegalDocument();

            legalDocument.TermsOfService = true;

            legalDocument.TypedOutAcceptance = "I Agree";
        }

        void validating_acceptance()
        {
            act = () => isValid = legalDocument.IsValid();

            context["terms of service is set to true"] = () =>
            {
                before = () => legalDocument.TermsOfService = true;

                it["the specific property is valid"] = () => ((bool)legalDocument.IsValid("TermsOfService")).should_be_true();

                it["the object is valid"] = () => isValid.should_be_true();
            };
            
            context["terms of service is set to false"] = () =>
            {
                before = () => legalDocument.TermsOfService = false;

                it["is valid"] = () => isValid.should_be_false();
            };

            context["acceptance criteria is a string match for 'TypedOutAcceptance'"] = () =>
            {
                context["user types out I Accept"] = () =>
                {
                    before = () => legalDocument.TypedOutAcceptance = "I Agree";

                    it["is valid"] = () => isValid.should_be_true();
                };
            };
        }

        void error_message()
        {
            act = () =>
            {
                legalDocument.IsValid();

                error = legalDocument.FirstError();
            };

            context["Terms of service has default error message and has not been accepted."] = () =>
            {
                before = () => legalDocument.TermsOfService = false;

                it["error message reads 'TermsOfService is invalid.'"] = () => error.should_be("TermsOfService is invalid.");
            };

            context["Typed out acceptances has customized error message and is not valid"] = () =>
            {
                before = () => legalDocument.TypedOutAcceptance = "I disagree";

                it["error message reads 'You have not typed out the acceptence. Type I Accept.'"] = () =>
                    error.should_be("You have not typed out the acceptance. Type I Accept.");
            };
        }
    }
}
