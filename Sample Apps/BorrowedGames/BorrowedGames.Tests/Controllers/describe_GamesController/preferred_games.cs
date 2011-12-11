using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace BorrowedGames.Tests.Controllers.describe_GamesController
{
    [Tag("describe_User,describe_GamesController")]
    class preferred_games : _games_controller
    {
        void retrieving_preferred_games()
        {
            act = () => result = controller.Preferred();

            context["given user is following a user who has games"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(userId, followingId, mirrorsEdgeId);

                it["contains a list of preferred games"] = () =>
                {
                    PreferredGames().Count().should_be(1);
                    (FirstPreferredGame().Id as object).should_be(mirrorsEdgeId as object);
                    (FirstPreferredGame().Name as string).should_be("Mirror's Edge");
                };

                it["contains a hypermedia link to not interested games"] = () => 
                {
                    int gameId = FirstPreferredGame().Id;
                    (FirstPreferredGame().NotInterested as string).should_be("/Games/NotInterested/?gameId=" + gameId);
                };

                context["user owns a game that a friend has"] = () =>
                {
                    before = () => GivenUserHasGame(userId, mirrorsEdgeId);

                    it["doesn't contain game"] = () => PreferredGames().Count().should_be(0);
                };
            };

            context["user is not following anyone"] = () =>
            {
                it["contains no games"] = () => PreferredGames().Count().should_be(0);
            };
        }
    }
}
