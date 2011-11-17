using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Models;
using Oak;
using Oak.Controllers;

namespace BorrowedGames.Tests.Models
{
    class describe_Registration : nspec
    {
        dynamic registration;

        bool isValid;

        SeedController seedController;

        void before_each()
        {
            seedController = new SeedController();

            seedController.PurgeDb();

            seedController.All();

            registration = ValidRegistration();
        }

        void registration_validation()
        {
            act = () => isValid = registration.IsValid();

            context["valid registration"] = () =>
            {
                before = () => registration = ValidRegistration();

                it["is valid"] = () => isValid.should_be_true();
            };

            context["email is not specified"] = () =>
            {
                before = () => registration.Email = default(string);

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["email is has invalid format"] = () =>
            {
                before = () => registration.Email = "user";

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["password is not specified"] = () =>
            {
                before = () => registration.Password = default(string);

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["password must be confirmed"] = () =>
            {
                before = () =>
                {
                    registration.Password = "password";
                    registration.PasswordConfirmation = "";
                };

                it["is invalid"] = () => isValid.should_be_false();
            };
        }

        dynamic ValidRegistration()
        {
            return new Registration(new
            {
                Email = "user@example.com",
                Password = "password",
                PasswordConfirmation = "password"
            });
        }
    }
}
