using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Gemini
{
    public class PersonToExtend : Gemini
    {
    }

    [Tag("wip")]
    class extending_gemini : nspec
    {
        dynamic gemini;

        void before_each()
        {
            gemini = new Gemini();
        }

        void specify_an_instance_of_a_gemini_can_be_extended()
        {
            ((bool)gemini.RespondsTo("Changes")).should_be_false();

            gemini.Extend<Changes>();

            ((bool)gemini.RespondsTo("Changes")).should_be_true();
        }

        void specify_a_class_can_be_extended_globally()
        {
            dynamic personToExtend = new PersonToExtend();

            ((bool)personToExtend.RespondsTo("Changes")).should_be_false();

            Gemini.Extend<PersonToExtend, Changes>();

            personToExtend = new PersonToExtend();

            ((bool)personToExtend.RespondsTo("Changes")).should_be_true();
        }
    }
}
