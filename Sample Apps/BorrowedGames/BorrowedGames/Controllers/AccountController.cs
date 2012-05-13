using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using BorrowedGames.Models;
using System.Text;
using System.Security.Cryptography;
using System.Net.Mail;

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

            if (user == null || user.Password != Registration.Encrypt(@params.Password))
            {
                ViewBag.Flash = "Login failed.";

                return View();
            }

            Authenticate(user.Email);

            return Redirect(@params.RedirectUrl ?? "/");
        }

        [HttpPost]
        public dynamic Register(dynamic @params)
        {
            dynamic registration = new Registration(@params);

            string unalteredPassword = @params.Password;

            if (!registration.IsValid())
            {
                ViewBag.Flash = registration.FirstError();

                return View();
            }

            registration.Register();

            return LogOn(new { @params.Email, Password = unalteredPassword, RedirectUrl = "/" });
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();

            return RedirectToAction("LogOn");
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
