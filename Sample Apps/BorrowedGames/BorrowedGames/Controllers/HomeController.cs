using System;
using System.Web.Mvc;
using BorrowedGames.Repositories;

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
            ViewBag.HasGames = User().HasGames();

            ViewBag.Handle = User().Handle;

            ViewBag.HasFriends = User().HasFriends();

            return View();
        }

        [HttpPost]
        public dynamic Handle(dynamic @params)
        {
            var user = User();

            user.Handle = @params.handle.ToLower();

            if(user.HasChanged())
            {
                if (!user.IsValid()) return Status(user.Changes("Handle").Original, user.FirstError());

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
