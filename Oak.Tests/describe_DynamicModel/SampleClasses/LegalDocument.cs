using System;
using Oak.Models;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
            Validates(new Acceptance { Property = "TermsOfService" });
            Validates(new Acceptance { Property = "TypedOutAcceptance", Accept = "I Agree" });
        }
    }
}
