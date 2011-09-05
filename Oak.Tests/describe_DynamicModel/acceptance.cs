using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.SampleClasses;

namespace Oak.Tests.describe_DynamicModel
{
    class acceptance : nspec
    {
        dynamic legalDocument;

        bool isValid;

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
    }
}
