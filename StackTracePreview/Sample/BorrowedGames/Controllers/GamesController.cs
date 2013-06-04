using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using Oak;
using BorrowedGames.Repositories;

namespace BorrowedGames.Controllers
{
    public class GamesController : BaseController
    {
        Games games = new Games();

        public dynamic Preferred()
        {
            return Json(LinksForPreferredGames(User().PreferredGames()));
        }

        public dynamic Wanted()
        {
            return Json(LinksForWantedGames(User().WantedGames()));
        }
        
        public dynamic Requested()
        {   
            return Json(LinksForRequestedGames(User().RequestedGames()));
        }

        public dynamic NotInterested()
        {
            return Json(LinksForNotInterestedGames(User().NotInterestedGames().Include("Game").ToList()));
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

        [HttpPost]
        public void UndoNotInterested(int gameId)
        {
            User().UndoNotInterestedGame(gameId);
        }

        [HttpPost]
        public void GiveGame(int gameId, int toUserId)
        {
            User().GiveGame(gameId, toUserId);
        }

        [HttpPost]
        public void GameReturned(int gameId, int byUserId)
        {
            User().GameReturned(gameId, byUserId);
        }

        public void Favorite(int gameId)
        {
            var game = games.Single(gameId);

            game.FavoritedBy(User());
        }

        public void Unfavorite(int gameId)
        {
            var game = games.Single(gameId);

            game.UnfavoritedBy(User());
        }

        public void ReturnGame(int gameId, int toUserId)
        {
            User().ReturnGame(gameId, toUserId);
        }

        public IEnumerable<dynamic> LinksForPreferredGames(IEnumerable<dynamic> games)
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

                if(s.IsFavorited)
                {
                    s.UnfavoriteGame = Url.RouteUrl(new
                    {
                        controller = "Games",
                        action = "Unfavorite",
                        gameId = s.Id
                    });    
                }
                else
                {
                    s.FavoriteGame = Url.RouteUrl(new
                    {
                        controller = "Games",
                        action = "Favorite",
                        gameId = s.Id
                    });
                }
            });

            return games;
        }

        public IEnumerable<dynamic> LinksForWantedGames(IEnumerable<dynamic> games)
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

                if (s.IsBorrowed())
                {
                    s.ReturnGame = Url.RouteUrl(new
                    {
                        controller = "Games",
                        action = "ReturnGame",
                        gameId = s.Id,
                        toUserId = s.Owner.Id
                    });    
                }
            });

            return games;
        }

        public IEnumerable<dynamic> LinksForNotInterestedGames(IEnumerable<dynamic> games)
        {
            games.ForEach(s =>
            {
                s.UndoNotInterested = Url.RouteUrl(new
                {
                    controller = "Games",
                    action = "UndoNotInterested",
                    gameId = s.GameId
                });
            });

            return games;
        }

        public IEnumerable<dynamic> LinksForRequestedGames(IEnumerable<dynamic> games)
        {
            games.ForEach(s =>
            {
                if(s is RequestedGame)
                {
                    s.GiveGame = Url.RouteUrl(new
                    {
                        controller = "Games",
                        action = "GiveGame",
                        gameId = s.Id,
                        toUserId = s.RequestedBy.Id
                    });
                }

                if(s is LendedGame)
                {
                    s.GameReturned = Url.RouteUrl(new
                    {
                        controller = "Games",
                        action = "GameReturned",
                        gameId = s.Id,
                        byUserId = s.RequestedBy.Id
                    });
                }
            });

            return games;
        }
    }
}
