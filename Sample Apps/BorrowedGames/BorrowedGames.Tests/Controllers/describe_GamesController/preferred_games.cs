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
        [Tag("describe_Game")]
        void favoriting_a_preferred_game()
        {
            before = () =>
            {
                GivenUserHasFriendWithGame(currentUserId, friendId, gearsOfWarId);

                GivenUserHasFriendWithGame(currentUserId, friendId, mirrorsEdgeId);
            };

            act = () => controller.Favorite(mirrorsEdgeId);

            it["favorited games are listed first"] = () =>
            {
                ((bool)FirstPreferredGame().IsFavorited).should_be_true();

                (FirstPreferredGame().Id as object).should_be(mirrorsEdgeId as object);
            };

            it["contains a link to unfavorite the game"] = () =>
                (FirstPreferredGame().UnfavoriteGame as string).should_be("/Games/Unfavorite?gameId=" + mirrorsEdgeId);

            it["doesn't contain a link to favorite the game"] = () =>
                ((bool)FirstPreferredGame().RespondsTo("FavoriteGame")).should_be_false();

            context["game is unfavorited"] = () =>
            {
                act = () => controller.Unfavorite(mirrorsEdgeId);

                it["the list is returned to the default order"] = () =>
                    (FirstPreferredGame().Id as object).should_be(gearsOfWarId as object);

                it["a link is provided to mark the game as favorite"] = () =>
                    (FirstPreferredGame().FavoriteGame as string).should_be("/Games/Favorite?gameId=" + gearsOfWarId);
            };
        }

        void retrieving_preferred_games()
        {
            act = () => result = controller.Preferred();

            context["given user is following a user who has games"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(currentUserId, friendId, mirrorsEdgeId);

                it["contains a list of preferred games"] = () =>
                {
                    PreferredGames().Count().should_be(1);
                    (FirstPreferredGame().Id as object).should_be(mirrorsEdgeId as object);
                    (FirstPreferredGame().Name as string).should_be("Mirror's Edge");
                };

                it["contains a hypermedia link to not interested games"] = () =>
                {
                    int gameId = FirstPreferredGame().Id;
                    (FirstPreferredGame().NotInterested as string).should_be("/Games/NotInterested?gameId=" + gameId);
                };

                it["contains a hypermedia link to request game"] = () =>
                {
                    int gameId = FirstPreferredGame().Id;
                    (FirstPreferredGame().WantGame as string).should_be("/Games/WantGame?gameId=" + gameId + "&followingId=" + friendId);
                };

                context["user owns a game that a friend has"] = () =>
                {
                    before = () => GivenUserHasGame(currentUserId, mirrorsEdgeId);

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
