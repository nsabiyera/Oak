using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_GamesController")]
    [Tag("describe_User")]
    [Tag("describe_WantedGame")]
    class requested_games : _games_controller
    {
        void retrieving_requested_games()
        {
            context["friend has requested games from user"] = () =>
            {
                before = () => GivenUserWantsGame(friendId, fromUser: currentUserId, game: mirrorsEdgeId);

                it["contains the friends request for game"] = () => 
                    ((int)FirstRequestedGame().Id).should_be(mirrorsEdgeId);

                it["contains the handle of the friend that requested the game"] = () => 
                    ((string)FirstRequestedGame().RequestedBy.Handle).should_be("@following");
            };

            context["user has games, but none have been requested"] = () =>
            {
                before = () => GivenUserHasGame(currentUserId, mirrorsEdgeId);

                it["requested games is empty"] = () => RequestedGames().should_be_empty();
            };
        }
    }
}
