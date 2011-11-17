using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using System.Text.RegularExpressions;

namespace BorrowedGames.Controllers
{
    [Authorize]
    public class HomeController : BaseController
    {
        Library library;

        public HomeController()
        {
            library = new Library();
        }

        public dynamic Index()
        {
            ViewBag.HasGames = (User().Games() as IEnumerable<dynamic>).Count() > 0;

            ViewBag.Handle = User().Handle;

            ViewBag.HasFriends = (User().Friends() as IEnumerable<dynamic>).Count() > 0;

            return View();
        }

        [HttpPost]
        public dynamic Handle(dynamic @params)
        {
            var user = User();

            var originalHandle = user.Handle;

            user.Handle = @params.handle.ToLower();

            if(user.HasChanged())
            {
                if (!user.IsValid()) return Status(originalHandle, user.FirstError());

                users.Save(user);
            }

            return Status(user.Handle, "Your handle has been updated to " + user.Handle + ".");
        }

        public dynamic Status(string handle, string message)
        {
            return Json(new { Handle = handle, Message = message });
        }
    }
}
