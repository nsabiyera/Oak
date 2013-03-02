using System;
using NSpec;
using System.Collections.Generic;

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
  SetMembers (DynamicFunctionWithParam)
  FirstName (String): Jane
  LastName (String): Doe
");
        }

        void specify_requesting_info_for_null_values_prints_null()
        {
            gemini.Body = null;

            var info = gemini.__Info__() as string;

            info.should_be(@"this (Gemini)
  SetMembers (DynamicFunctionWithParam)
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
  SetMembers (DynamicFunctionWithParam)
  FirstName (String): Jane
  LastName (String): Doe
  SayHello (DynamicFunction)
");
        }

        void specify_prototype_acts_like_gemini()
        {
            var info = gemini.Prototype.ToString() as string;

            info.should_be(@"this (Prototype)
  SetMembers (DynamicFunctionWithParam)
  FirstName (String): Jane
  LastName (String): Doe
");
        }

        void specify_circular_references()
        {
            dynamic otherFriend = new Gemini(new { FirstName = "Jane", LastName = "Doe" });

            dynamic friend = new Gemini(new { FirstName = "John", LastName = "Doe", Friend = otherFriend });

            otherFriend.Friend = friend;

            var info = friend.__Info__() as string;

            info.should_be(@"this (Gemini)
  FirstName (String): John
  LastName (String): Doe
  Friend (Gemini)
    FirstName (String): Jane
    LastName (String): Doe
    SetMembers (DynamicFunctionWithParam)
    Friend (circular)
  SetMembers (DynamicFunctionWithParam)
");
        }

        void specify_to_string_is_the_same_as_info()
        {
            (gemini.ToString() as string).should_be(gemini.__Info__() as string);
        }

        void specify_structs_are_printed()
        {
            gemini = new Gemini(new { Struct = new KeyValuePair<string, string>("Name", "Jane Doe") });

            var info = gemini.__Info__() as string;

            info.should_be(@"this (Gemini)
  Struct (KeyValuePair`2): [Name, Jane Doe]
  SetMembers (DynamicFunctionWithParam)
");
        }
    }
}
