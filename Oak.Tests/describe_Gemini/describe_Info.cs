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

        void specify_requesting_info_shows_properties()
        {
            var info = gemini.__Info__() as string;

            info.should_be(@"this (Gemini)
  FirstName (String): Jane
  LastName (String): Doe
");
        }

        void specify_requesting_info_for_null_values_prints_null()
        {
            gemini.Body = null;

            var info = gemini.__Info__() as string;

            info.should_be(@"this (Gemini)
  FirstName (String): Jane
  LastName (String): Doe
  Body (null)
");
        }

        void specify_requesting_info_shows_delegates()
        {
            gemini.SayHello = new DynamicFunction(() => "hi");

            var info = gemini.__Info__() as string;

            info.should_be(@"this (Gemini)
  FirstName (String): Jane
  LastName (String): Doe
  SayHello (DynamicFunction)
");
        }

        void specify_to_string_is_the_same_as_info()
        {
            (gemini.ToString() as string).should_be(gemini.__Info__() as string);
        }
    }
}
