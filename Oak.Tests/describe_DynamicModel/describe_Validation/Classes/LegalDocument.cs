using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_Validation.Classes
{
    public class LegalDocument : DynamicModel
    {
        public LegalDocument()
        {
            Init();
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Acceptance("TermsOfService");

            yield return new Acceptance("TypedOutAcceptance")
            {
                Accept = "I Agree",
                Text = "You have not typed out the acceptance. Type I Accept."
            };
        }
    }
}
