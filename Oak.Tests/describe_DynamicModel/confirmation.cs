using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.SampleClasses;

namespace Oak.Tests.describe_DynamicModel
{
    class confirmation : nspec
    {
        dynamic registration;

        bool result;

        void before_each()
        {
            registration = new Registration();

            registration.Email = "user@example.com";
            registration.Password = "Password";
            registration.PasswordConfirmation = "Password";
        }

        void confirming_password_is_entered()
        {
            act = () => result = registration.IsValid();

            context["given passwords match"] = () =>
            {
                before = () =>
                {
                    registration.Password = "Password";
                    registration.PasswordConfirmation = "Password";
                };

                it["is valid"] = () => result.should_be_true();
            };

            context["given passwords do not match"] = () =>
            {
                before = () =>
                {
                    registration.Password = "Password";
                    registration.PasswordConfirmation = "password";
                };

                it["is invalid"] = () => result.should_be_false();
            };
        }
    }
}
