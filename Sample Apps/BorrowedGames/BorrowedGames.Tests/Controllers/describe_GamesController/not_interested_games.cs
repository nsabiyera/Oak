using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_User,describe_GamesController,describe_NotInterestedGame")]
    class not_interested_games : _games_controller
    {
        void marking_a_game_as_not_interested()
        {
            act = () => controller.NotInterested(mirrorsEdgeId);

            context["given user has preferred games"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(currentUserId, isFollowing: friendId, whoHasGame: mirrorsEdgeId);

                it["games marked as not interested do not show up on preferred games list"] = () =>
                    PreferredGames().Count().should_be(0);
            };
        }

        void retrieving_not_interested_games()
        {
            context["a game is marked as not interested"] = () => 
            {
                act = () => controller.NotInterested(gearsOfWarId);

                it["the list of not interested games contains the game that was marked"] = () =>
                    (FirstNotInterestedGame().Name as string).should_be("Gears of War");

                it["the games contain links to undo the not interested action"] = () =>
                    (FirstNotInterestedGame().UndoNotInterested as string).should_be("/Games/UndoNotInterested?gameId=" + gearsOfWarId);

                it["the games contain console information"] = () =>
                    (FirstNotInterestedGame().Console as string).should_be("XBOX360");
            };
        }

        void undo_not_interested_game()
        {
            act = () =>
            {
                controller.NotInterested(mirrorsEdgeId);

                (FirstNotInterestedGame().Name as string).should_be("Mirror's Edge");

                controller.UndoNotInterested(mirrorsEdgeId);
            };

            it["the game is removed from the not interested list"] = () =>
                NotInterestedGames().should_be_empty();
        }
    }
}
