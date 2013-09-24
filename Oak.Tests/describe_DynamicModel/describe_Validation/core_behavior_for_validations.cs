using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class core_behavior_for_validations : nspec
    {
        dynamic gemini;

        void validation_can_be_added_directly_to_gemini()
        {
            before = () =>
            {
                gemini = new Gemini();

                gemini.Validates = new DynamicFunction(() =>
                {
                    return new []
                    {
                        new Presence("FirstName")
                    };
                });

                new Validations(gemini);
            };

            it["validation methods exist when validation is mixed in"] = () =>
                ((bool)gemini.IsValid()).should_be(false);
        }

        void specify_is_valid_is_true_if_validates_isnt_given()
        {
            gemini = new Gemini();

            new Validations(gemini);

            ((bool)gemini.IsValid()).should_be(true); 
        }

        void specify_a_detailed_exception_is_thrown_if_the_inititalization_for_validation_fails()
        {
            gemini = new FailedValidation();

            try
            {
                gemini.IsValid();
            }
            catch (Exception ex)
            {
                ex.Message.should_contain("Validation initialization failed for class FailedValidation.  Check the Validates method on FailedValidation for a validation declaration related to this exception: ");

                ex.Message.should_contain("APropertyThatDoesntExist");
            }
        }

        void specify_validations_error_message_can_be_a_delegate_to_allow_for_deferred_error_messages()
        {
            gemini = new DeferredFailedValidation();

            gemini.FirstName = "";

            gemini.DeferredProp = "Hello";

            gemini.IsValid();

            (gemini.FirstError() as string).should_be("Hello");
        }

        void specify_first_error_returns_null_if_no_errors()
        {
            gemini = new DeferredFailedValidation();

            gemini.FirstName = "Foobar";

            gemini.IsValid();

            ((object)gemini.FirstError()).should_be_null();
        }
    }
}
