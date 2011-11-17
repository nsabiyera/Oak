using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Controllers;
using BorrowedGames.Models;

namespace BorrowedGames.Tests.Controllers
{
    [Tag("describe_User")]
    class describe_GamesController : _borrowed_games
    {
        GamesController controller;

        dynamic result;

        int mirrorsEdgeId, userId, followingId, anotherUserId;

        void before_each()
        {
            controller = new GamesController();

            MockSession(controller);

            userId = GivenUser("user@example.com");

            followingId = GivenUser("following@example.com");

            anotherUserId = GivenUser("another@example.com");

            controller.CurrentUser = userId;

            mirrorsEdgeId = GivenGame("Mirror's Edge");
        }

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

        void requesting_game()
        {
            before = () =>
            {
                GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: mirrorsEdgeId);

                ((bool)FirstPreferredGame().Requested).should_be_false();
            };

            act = () => controller.RequestGame(mirrorsEdgeId, followingId);

            it["markes the game as requested"] = () =>
                    ((bool)FirstPreferredGame().Requested).should_be_true();
        }

        dynamic FirstPreferredGame()
        {
            return PreferredGames().First();
        }

        IEnumerable<dynamic> PreferredGames()
        {
            return controller.Preferred().Data;
        }
    }
}
