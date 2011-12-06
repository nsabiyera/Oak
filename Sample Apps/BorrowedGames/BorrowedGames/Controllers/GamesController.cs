using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BorrowedGames.Controllers
{
    public class GamesController : BaseController
    {
        public dynamic Preferred()
        {
            return Json(User().PreferredGames());
        }

        [HttpPost]
        public void NotInterested(int gameId)
        {
            User().MarkGameNotInterested(gameId);
        }
        
        [HttpPost]
        public void RequestGame(int gameId, int followingId)
        {
            User().RequestGame(gameId, followingId);
        }

        [HttpPost]
        public void DeleteRequest(int gameId, int followingId)
        {
            User().DeleteGameRequest(gameId, followingId);
        }
    }
}
