using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using Oak;

namespace BorrowedGames.Controllers
{
    public class GamesController : BaseController
    {
        WantedGames wantedGames = new WantedGames();

        public dynamic Preferred()
        {
            return Json(AddHyperMediaLinksForPreferred(User().PreferredGames()));
        }

        public dynamic Wanted()
        {
            return Json(AddHyperMediaLinksForWanted(User().WantedGames()));
        }

        public dynamic Requested()
        {
            dynamic requested = wantedGames.All(where: "FromUserId = @0", args: new object[] { CurrentUser });

            return Json(requested.Games());
        }

        public IEnumerable<dynamic> AddHyperMediaLinksForPreferred(List<dynamic> games)
        {
            games.ForEach(s =>
            {
                s.NotInterested = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "NotInterested",
                    gameId = s.Id
                });

                s.WantGame = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "WantGame",
                    gameId = s.Id,
                    followingId = s.Owner.Id
                });
            });

            return games;
        }

        public IEnumerable<dynamic> AddHyperMediaLinksForWanted(List<dynamic> games)
        {
            games.ForEach(s => 
            {
                s.DeleteWant = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "DeleteWant",
                    gameId = s.Id,
                    followingId = s.Owner.Id
                });
            });

            return games;
        }

        [HttpPost]
        public void NotInterested(int gameId)
        {
            User().MarkGameNotInterested(gameId);
        }

        [HttpPost]
        public void WantGame(int gameId, int followingId)
        {
            User().WantGame(gameId, followingId);
        }

        [HttpPost]
        public void DeleteWant(int gameId, int followingId)
        {
            User().DeleteWantedGame(gameId, followingId);
        }
    }
}
