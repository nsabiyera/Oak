using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Acceptance("TermsOfService") { ErrorMessage = new DynamicFunction(() => "Terms of service requires acceptance") };

            yield return new Acceptance("TypedOutAcceptance")
            {
                Accept = "I Agree",
                ErrorMessage = "You have not typed out the acceptance. Type I Accept."
            };
        }
    }

    public class LegalDocumentWithAutoProps : DynamicModel
    {
        public bool TermsOfService { get; set; }

        public string TypedOutAcceptance { get; set; }

        public LegalDocumentWithAutoProps()
            : base(new { })
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Acceptance("TermsOfService") { ErrorMessage = new DynamicFunction(() => "Terms of service requires acceptance") };

            yield return new Acceptance("TypedOutAcceptance")
            {
                Accept = "I Agree",
                ErrorMessage = "You have not typed out the acceptance. Type I Accept."
            };
        }
    }
}
