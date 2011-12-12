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

        public int mirrorsEdgeId, userId, followingId, anotherUserId, gearsOfWarId;

        void before_each()
        {
            controller = new GamesController();

            MockSession(controller);

            MockRouting(controller);

            userId = GivenUser("user@example.com");

            followingId = GivenUser("following@example.com", "@following");

            anotherUserId = GivenUser("another@example.com");

            controller.CurrentUser = userId;

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

        public IEnumerable<dynamic> PreferredGames()
        {
            return controller.Preferred().Data;
        }

        public IEnumerable<dynamic> RequestedGames()
        {
            return controller.Requested().Data;
        }

        public bool IsRequested(int gameId, int userId)
        {
            if (PreferredGame(gameId, userId) == null) return true;

            else return false;
        }
    }
}
