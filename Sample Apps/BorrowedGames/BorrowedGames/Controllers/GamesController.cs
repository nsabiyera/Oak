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
            return Json(AddNotInterestedLink(User().PreferredGames().ToList()));
        }

        public IEnumerable<dynamic> AddNotInterestedLink(List<dynamic> games)
        {
            games.ForEach(s => s.NotInterested = "/Games/NotInterested/?gameId=" + s.Id);

            return games;
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

        public dynamic Requested()
        {
            return Json(User().RequestedGames());
        }
    }
}
