using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class FailedValidation : Gemini
    {
        static FailedValidation()
        {
            Gemini.Initialized<FailedValidation>(d =>
            {
                d.Extend<Validations>();
            });
        }

        IEnumerable<dynamic> Validates()
        {
            yield return new Presence("FirstName") { ErrorMessage = _.APropertyThatDoesntExist };
        }
    }

    public class DeferredFailedValidation : Gemini
    {
        static DeferredFailedValidation()
        {
            Gemini.Initialized<DeferredFailedValidation>(d =>
            {
                d.Extend<Validations>();
            });
        }

        IEnumerable<dynamic> Validates()
        {
            yield return new Presence("FirstName") { ErrorMessage = new DynamicFunction(() => _.DeferredProp) };
        }
    }
}
