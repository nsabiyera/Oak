using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BorrowedGames.Controllers;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    class _games_controller : _borrowed_games
    {
        public GamesController controller;

        public int mirrorsEdgeId, currentUserId, friendId, anotherFriendId, gearsOfWarId;

        void before_each()
        {
            controller = new GamesController();

            MockRouting(controller);

            currentUserId = GivenUser("user@example.com", "@current");

            friendId = GivenUser("following@example.com", "@following");

            anotherFriendId = GivenUser("another@example.com");

            SetCurrentUser(controller, currentUserId);

            mirrorsEdgeId = GivenGame("Mirror's Edge");

            gearsOfWarId = GivenGame("Gears of War");
        }

        public dynamic PreferredGame(int gameId, int userId)
        {
            return PreferredGames().FirstOrDefault(s => s.Id == gameId && s.Owner.Id == userId);
        }

        public dynamic FirstPreferredGame()
        {
            return PreferredGames().First();
        }

        public dynamic FirstRequestedGame(int gameId, int requestedById)
        {
            return FirstRequestedGame(s => s.Id == gameId && s.RequestedBy.Id == requestedById);
        }

        public dynamic FirstRequestedGame(Func<dynamic, bool> where = null)
        {
            where = where ?? new Func<dynamic, bool>(d => true);

            return RequestedGames().First(where);
        }

        public dynamic FirstNotInterestedGame()
        {
            return NotInterestedGames().First();
        }

        public IEnumerable<dynamic> RequestedGames()
        {
            return controller.Requested().Data;
        }

        public IEnumerable<dynamic> PreferredGames()
        {
            return controller.Preferred().Data;
        }

        public dynamic FirstWantedGame(int userId)
        {
            return WantedGames(userId).First();
        }

        public IEnumerable<dynamic> WantedGames(int userId)
        {
            return User(userId).WantedGames();
        }

        public IEnumerable<dynamic> NotInterestedGames()
        {
            return controller.NotInterested().Data;
        }

        public bool IsWanted(int gameId, int userId)
        {
            if (PreferredGame(gameId, userId) == null) return true;

            else return false;
        }
    }
}
