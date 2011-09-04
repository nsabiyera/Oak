using System;
using Oak.Models;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
            Validates("TermsOfService", Acceptance);
            Validates("TypedOutAcceptance", Acceptance, new { accept = "I Agree" });
        }
    }
}
