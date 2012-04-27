using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("decribe_User")]
    class returning_game : _games_controller
    {
        int borrowedFromUserId;

        void a_game_has_been_given()
        {
            before = () =>
            {
                borrowedFromUserId = currentUserId;

                GivenUserWantsGame(anotherFriendId, fromUser: borrowedFromUserId, game: gearsOfWarId);

                GivenUserWantsGame(friendId, fromUser: borrowedFromUserId, game: gearsOfWarId);

                GivenGameIsBorrowed(game: gearsOfWarId, fromUser: borrowedFromUserId, givenTo: friendId);
            };

            context["user who originally gave game marks the game as returned"] = () => 
            {
                act = () => controller.GameReturned(gearsOfWarId, friendId);

                it["the game is no longer borrowed"] = () =>
                    BorrowedGames(friendId).should_be_empty();

                it["game is no longer wanted"] = () =>
                    WantedGames(friendId).should_be_empty();

                it["wanted games for other friends are not affected"] = () =>
                    WantedGames(anotherFriendId).should_contain(s => s.Id == gearsOfWarId);
            };

            context["user who borrowed game marks the game as returned"] = () =>
            {
                before = () => controller.CurrentUser = friendId;

                act = () => controller.ReturnGame(gearsOfWarId, borrowedFromUserId);

                it["the game is no longer borrowed"] = () =>
                    BorrowedGames(friendId).should_be_empty();

                it["game is no longer wanted"] = () =>
                    WantedGames(friendId).should_be_empty();

                it["wanted games for other friends are not affected"] = () =>
                    WantedGames(anotherFriendId).should_contain(s => s.Id == gearsOfWarId);
            };
        }
    }
}
