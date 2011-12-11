using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_User,describe_GamesController")]
    class not_interested_games : _games_controller
    {
        void marking_a_game_as_not_interested()
        {
            act = () => controller.NotInterested(mirrorsEdgeId);

            context["given user has preferred games"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: mirrorsEdgeId);

                it["games marked as not interested do not show up on preferred games list"] = () =>
                    PreferredGames().Count().should_be(0);
            };
        }
    }
}
