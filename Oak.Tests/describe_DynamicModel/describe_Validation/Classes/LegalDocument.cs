using System;

namespace Oak.Tests.describe_Validation.Classes
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
            Validates(new Acceptance("TermsOfService"));

            Validates(new Acceptance("TypedOutAcceptance")
            {
                Accept = "I Agree",
                Text = "You have not typed out the acceptance. Type I Accept."
            });
        }
    }
}
