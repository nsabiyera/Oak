using System;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
            Validates(new Acceptance { Property = "TermsOfService" });

            Validates(new Acceptance
            {
                Property = "TypedOutAcceptance",
                Accept = "I Agree",
                Text = "You have not typed out the acceptance. Type I Accept."
            });
        }
    }
}
