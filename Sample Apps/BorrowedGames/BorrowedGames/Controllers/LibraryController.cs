using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;

namespace BorrowedGames.Controllers
{
    public class LibraryController : BaseController
    {
        Library library;

        Games games;

        Func<dynamic, dynamic> JsonGame = s => new { s.Id, s.Name };

        public LibraryController()
        {
            library = new Library();

            games = new Games();
        }

        public dynamic List()
        {
            return Json(GamesInLibraryFor(CurrentUser));
        }

        public dynamic ListFor(dynamic user)
        {
            return Json(GamesInLibraryFor(user));
        }

        [HttpPost]
        public dynamic Add(dynamic @params)
        {
            if (!UserHasGame(@params.gameId)) library.Insert(new { UserId = CurrentUser, GameId = @params.gameId });

            return Json(JsonGame(games.Single(@params.gameId)));
        }

        [HttpPost]
        public void Delete(dynamic @params)
        {
            if (UserHasGame(@params.gameId)) library.Delete(LibraryEntry(@params.gameId).Id);
        }

        public dynamic Search(string searchString)
        {
            var searchResults = games.StartsWith(searchString);

            if (searchResults.Count() == 0) searchResults = games.Contains(searchString);

            if (searchResults.Count() == 0) searchResults = games.HasWords(searchString);

            return Json(searchResults.Select(JsonGame));
        }

        private IEnumerable<dynamic> GamesInLibraryFor(dynamic user)
        {
            return (users.Single(user).Games() as IEnumerable<dynamic>).Select(JsonGame);
        }

        private bool UserHasGame(dynamic gameId)
        {
            return User().HasGame(new { Id = gameId });
        }

        private dynamic LibraryEntry(dynamic gameId)
        {
            return User().Library().First(new { GameId = gameId });
        }
    }
}
