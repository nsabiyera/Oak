using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

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
    }
}
