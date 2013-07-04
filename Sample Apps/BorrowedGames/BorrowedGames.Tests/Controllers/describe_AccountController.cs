using System;
using System.Linq;
using NSpec;
using BorrowedGames.Controllers;
using System.Web.Mvc;
using Rnwood.SmtpServer;
using System.Collections.Generic;

namespace BorrowedGames.Tests.Controllers
{
    class describe_AccountController : _borrowed_games
    {
        AccountController controller;

        dynamic user;

        bool authenticated;

        void before_each()
        {
            controller = new AccountController();

            controller.Authenticate = s =>
            {
                authenticated = true;

                SetCurrentUser(controller, Users.ForEmail(s).Id);
            };
        }

        void logging_in()
        {
            context["requesting login page"] = () =>
            {
                act = () => result = controller.LogOn();

                it["returns login page"] = () => (result as object).should_cast_to<ViewResult>();
            };

            context["authenticating"] = () =>
            {
                act = () => result = controller.LogOn(new
                {
                    Email = "user@example.com",
                    Password = "password",
                    RedirectUrl = null as string
                });

                context["user exists"] = () =>
                {
                    before = () => user = GivenUser("user@example.com", null, "password");

                    it["authenicates user"] = () => authenticated.should_be_true();

                    it["redirects to home page"] = () => (result.Url as string).should_be("/");

                    it["sets user in session"] = () => (controller.UserId()).should_be((decimal)user);
                };

                context["user exists, password doesn't match"] = () =>
                {
                    before = () => GivenUser("user@example.com", null, "other");

                    it["returns invalid login"] = () => (result.ViewBag.Flash as string).should_be("Login failed.");
                };

                context["user does not exist"] = () =>
                {
                    it["returns invalid login"] = () => (result.ViewBag.Flash as string).should_be("Login failed.");
                };
            };
        }

        void registering_for_site()
        {
            context["requesting registration page"] = () =>
            {
                act = () => result = controller.Register();

                it["returns view"] = () => (result as object).should_cast_to<ViewResult>();
            };

            context["user registers"] = () =>
            {
                act = () =>
                {
                    result = controller.Register(new
                    {
                        Email = "user@example.com",
                        Password = "password",
                        PasswordConfirmation = "password"
                    });

                    user = Users.All().First().Id;
                };

                it["logs in user"] = () => (result.Url as string).should_be("/");

                it["authenticates user"] = () => authenticated.should_be_true();

                it["sets user in session"] = () => ((decimal)controller.UserId()).should_be((decimal)user);

                context["user name is taken"] = () =>
                {
                    before = () => GivenUser("user@example.com");

                    it["return error stating that user name is taken"] = () =>
                        (result.ViewBag.Flash as string).should_be("Email is unavailable.");
                };
            };

            context["registration is invalid"] = () =>
            {
                act = () => result = controller.Register(new { Email = default(string), Password = default(string) });

                it["returns error stating that email is required."] = () =>
                    (result.ViewBag.Flash as string).should_be("Email is required.");
            };
        }
    }
}
