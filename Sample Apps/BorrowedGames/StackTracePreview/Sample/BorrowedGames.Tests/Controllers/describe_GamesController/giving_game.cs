using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Models;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_User,describe_WantedGame,describe_GamesController,describe_RequestedGame,describe_LendedGame")]
    class giving_game : _games_controller
    {
        void a_game_has_been_requested()
        {
            before = () =>
            {
                GivenUserWantsGame(anotherFriendId, fromUser: currentUserId, game: gearsOfWarId);

                GivenUserWantsGame(friendId, fromUser: currentUserId, game: gearsOfWarId);

                GivenUserWantsGame(friendId, fromUser: currentUserId, game: mirrorsEdgeId);
            };

            context["the requested game has been given"] = () =>
            {
                act = () => controller.GiveGame(gearsOfWarId, friendId);

                it["the games return date is set to one month from today"] = () =>
                {
                    ((DateTime?)FirstRequestedGame(gearsOfWarId, friendId).ReturnDate).should_be(OneMonthFromToday());
                    ((int)FirstRequestedGame(gearsOfWarId, friendId).DaysOut).should_be(0);
                    ((DateTime?)FirstWantedGame(friendId).ReturnDate).should_be(OneMonthFromToday());
                    ((int)FirstWantedGame(friendId).DaysLeft).should_be_greater_than(27);
                };

                it["the game is considered borrowed"] = () =>
                    ((int)FirstBorrowedGame(friendId).Id).should_be(gearsOfWarId);
                
                it["requested game cannot be given again (hypermedia link is removed)"] = () =>
                    ((bool)FirstRequestedGame(gearsOfWarId, friendId).RespondsTo("GiveGame")).should_be_false();

                it["the requested game can be marked as returned by the lender"] = () =>
                    ((string)FirstRequestedGame(gearsOfWarId, friendId).GameReturned).should_be("/Games/GameReturned?gameId=" + gearsOfWarId + "&byUserId=" + friendId);

                it["the user who gave the game doesn't have any borrowed games"] = () =>
                    BorrowedGames(currentUserId).Count().should_be(0);
            };

            context["the requested game has not been given yet"] = () =>
            {
                it["the game is not considered borrowed"] = () =>
                    BorrowedGames(friendId).Count().should_be(0);
            };
        }
    }
}
