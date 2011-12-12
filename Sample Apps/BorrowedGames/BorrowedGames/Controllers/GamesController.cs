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
            return Json(AddHyperMediaLinks(User().PreferredGames().ToList()));
        }

        public IEnumerable<dynamic> AddHyperMediaLinks(List<dynamic> games)
        {
            games.ForEach(s => 
            {
                s.NotInterested = Url.RouteUrl(new { controller = "Games", action = "NotInterested", gameId = s.Id });
                s.RequestGame = Url.RouteUrl(new { controller = "Games", action = "RequestGame", gameId = s.Id, followingId = s.Owner.Id });
            });

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
