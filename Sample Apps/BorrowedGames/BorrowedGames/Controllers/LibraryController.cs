using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BorrowedGames.Models;
using Oak;

namespace BorrowedGames.Controllers
{
    public class LibraryController : BaseController
    {
        Library library= new Library();

        Games games = new Games();

        FriendAssociations associations = new FriendAssociations();

        EmailHistorys emailHistorys = new EmailHistorys();

        public dynamic List()
        {
            return Json(GamesInLibraryFor(UserId()));
        }

        public dynamic ListFor(dynamic user)
        {
            return Json(GamesInLibraryFor(user));
        }

        [HttpPost]
        public dynamic Add(dynamic @params)
        {
            if (!UserHasGame(@params.gameId))
            {
                library.Insert(new { UserId = UserId(), GameId = @params.gameId });

                var user = User();

                SendNotificationEmails(user, 
                    games.Single(@params.gameId), 
                    user.Followers());
            }

            return Json(games.Single(@params.gameId));
        }

        private void SendNotificationEmails(dynamic user, dynamic game, IEnumerable<dynamic> followers)
        {
            if (emailHistorys.Exists(user.Id, DateTime.Today)) return;

            followers.ForEach(follower => SendGameAddedEmail(user, game, follower));
        }

        public void SendGameAddedEmail(dynamic user, dynamic game, dynamic follower)
        {
            var subject = user.Handle + " added " + game.Name + " (" + game.Console + ")" + " to his library.";

            dynamic email = new Email
            {
                To = follower.Email,
                Subject = subject,
                Body = subject + Environment.NewLine + "Go check it out at http://borrowedgames.com/"
            };

            emailHistorys.Insert(new { UserId = user.Id, CreatedAt = DateTime.Today });

            email.Send();
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

            return Json(searchResults);
        }

        private IEnumerable<dynamic> GamesInLibraryFor(dynamic user)
        {
            return (users.Single(user).Games() as IEnumerable<dynamic>);
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
