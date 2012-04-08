using System;
using NSpec;

namespace Oak.Tests.describe_Gemini
{
    class describe_Info : nspec
    {
        dynamic gemini;

        void before_each()
        {
            gemini = new Gemini();

            gemini.FirstName = "Jane";

            gemini.LastName = "Doe";
        }

        void specify_requesting_info_shows_methods()
        {
            var info = gemini.__Info__() as string;

            info.should_be("get__ (DynamicFunction), FirstName (String), LastName (String)");
        }

        void specify_info_can_be_greped()
        {
            var info = gemini.__Info__("Name") as string;

            info.should_be("FirstName (String), LastName (String)");
        }
    }
}
