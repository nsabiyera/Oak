using System.Collections.Generic;
using System.Linq;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    public class Persons : DynamicRepository
    {

    }

    class confirmation : nspec
    {
        Seed seed;  

        dynamic person;

        bool isValid;

        Persons persons;

        void before_each()
        {
            seed = new Seed();
            seed.PurgeDb();

            person = new Person();

            persons = new Persons();

            person.Email = "user@example.com";
            person.EmailConfirmation = "user@example.com";
        }

        void confirming_password_is_entered()
        {
            act = () => isValid = person.IsValid();

            context["given emails match"] = () =>
            {
                before = () =>
                {
                    person.Email = "user@example.com";
                    person.EmailConfirmation = "user@example.com";
                };

                it["is valid"] = () => isValid.should_be_true();
            };

            context["given emails do not match"] = () =>
            {
                before = () =>
                {
                    person.Email = "user@example.com";
                    person.EmailConfirmation = "dd";
                };

                it["is invalid"] = () => isValid.should_be_false();
            };
        }

        void saving_something_that_has_confirmation_to_the_database()
        {
            before = () =>
            {
                seed.CreateTable("Persons", new dynamic[]
                { 
                    new { Id = "int", PrimaryKey = true, Identity = true },
                    new { Email = "nvarchar(255)" }
                }).ExecuteNonQuery();
            };

            act = () =>
            {
                person.Email = "user@example.com";
                person.EmailConfirmation = "user@example.com";
            };

            it["requires the exclusion of confirmation properties"] = () =>
            {
                persons.Insert(person.Exclude("EmailConfirmation"));

                var firstPerson = persons.All().First();

                (firstPerson.Email as string).should_be(person.Email as string);
            };
        }
    }
}
