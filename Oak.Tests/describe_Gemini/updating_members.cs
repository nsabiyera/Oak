using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Gemini
{
    class updating_members : nspec
    {
        dynamic gemini;

        void it_only_updates_members_that_currently_exist()
        {
            gemini = new Gemini(new { FirstName = "Jane", LastName = "Doe" });

            gemini.UpdateMembers(new { FirstName = "J", LastName = "D", MiddleName = "None" });

            (gemini.FirstName as string).should_be("J");

            (gemini.LastName as string).should_be("D");

            ((bool)gemini.RespondsTo("MiddleName")).should_be(false);
        }
    }
}
