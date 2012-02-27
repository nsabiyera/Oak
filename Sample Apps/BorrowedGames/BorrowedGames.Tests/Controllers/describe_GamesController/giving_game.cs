using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_User,describe_WantedGame")]
    class giving_game : _games_controller
    {
        void a_game_has_been_requested()
        {
            before = () => GivenUserWantsGame(friendId, fromUser: currentUserId, game: mirrorsEdgeId);

            context["the requested game has been given"] = () =>
            {
                act = () => controller.GameGiven(mirrorsEdgeId, friendId);

                it["the games return date is set to one month from today"] = () =>
                ((DateTime)FirstRequestedGame().ReturnDate).should_be(OneMonthFromToday());

                it["the game is considered borrowed"] = () =>
                    ((int)FirstBorrowedGame(friendId).Id).should_be(mirrorsEdgeId);

                it["the user who gave the game doesn't have any borrowed games"] = () =>
                    BorrowedGames(currentUserId).Count().should_be(0);
            };

            context["the requested game has not been given yet"] = () =>
            {
                it["the game is not considered borrowed"] = () =>
                    BorrowedGames(friendId).Count().should_be(0);
            };
        }

        public dynamic FirstBorrowedGame(int userId)
        {
            return BorrowedGames(userId).First();
        }

        public IEnumerable<dynamic> BorrowedGames(int userId)
        {
            return User(userId).BorrowedGames() as IEnumerable<dynamic>;
        }
    }
}
