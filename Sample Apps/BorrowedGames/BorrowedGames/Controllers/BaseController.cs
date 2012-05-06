using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using System.Dynamic;
using JsonFx.Json;
using Oak;

namespace BorrowedGames.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class BaseController : Controller
    {
        public Func<string, dynamic> GetSessionValue { get; set; }

        public Action<string, dynamic> SetSessionValue { get; set; }

        public Func<string> Email { get; set; }

        protected Users users;

        public BaseController()
        {
            users = new Users();

            GetSessionValue = (key) => HttpContext.Session[key];

            SetSessionValue = (key, value) => HttpContext.Session[key] = value;

            Email = () => base.User.Identity.Name;
        }

        public new JsonResult Json(object o)
        {
            return new DynamicJsonResult(o);
        }

        public new dynamic User()
        {
            return users.ForEmail(Email());
        }

        public int UserId()
        {
            return Convert.ToInt32(User().Id);
        }

        public dynamic Friends()
        {
            return User().Friends();
        }
    }
}
