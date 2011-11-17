using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BorrowedGames.Models;

namespace BorrowedGames.Controllers
{
    public class AccountController : BaseController
    {
        public Action<string> Authenticate { get; set; }

        public AccountController()
        {
            Authenticate = email => FormsAuthentication.RedirectFromLoginPage(email, false);
        }

        public dynamic LogOn()
        {
            return View();
        }

        [HttpPost]
        public dynamic LogOn(dynamic @params)
        {
            var user = GetUser(@params);

            if (user == null || user.Password != @params.Password)
            {
                ViewBag.Flash = "Login failed.";

                return View();
            }

            Authenticate(user.Email);

            CurrentUser = user.Id;

            return Redirect(@params.RedirectUrl ?? "/");
        }

        [HttpPost]
        public dynamic Register(dynamic @params)
        {
            dynamic registration = new Registration(@params);

            if (!registration.IsValid())
            {
                ViewBag.Flash = registration.FirstError();

                return View();
            }

            users.Insert(registration);

            return LogOn(new { @params.Email, @params.Password, RedirectUrl = "/" });
        }

        public dynamic Register()
        {
            return View();
        }

        private dynamic GetUser(dynamic @params)
        {
            return users.SingleWhere("Email = @0", @params.Email);
        }
    }
}
