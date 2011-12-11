using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Controllers;
using BorrowedGames.Models;
using BorrowedGames.Tests.Controllers.describe_GamesController;

namespace BorrowedGames.Tests.Controllers
{
    [Tag("describe_User,describe_GamesController")]
    class requested_games : _games_controller
    {
        void requesting_game()
        {
            before = () =>
            {
                GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: mirrorsEdgeId);

                IsRequested(mirrorsEdgeId, userId).should_be_true();
            };

            act = () => controller.RequestGame(mirrorsEdgeId, followingId);

            it["marks the game as requested"] = () =>
                IsRequested(mirrorsEdgeId, followingId).should_be_true();
        }

        void unrequesting_game_for_a_specific_user()
        {
            before = () =>
            {
                GivenUserHasRequestedGame(userId, fromUser: followingId, game: gearsOfWarId);

                GivenUserHasRequestedGame(userId, fromUser: anotherUserId, game: mirrorsEdgeId);

                GivenUserHasRequestedGame(userId, fromUser: followingId, game: mirrorsEdgeId);
            };

            act = () => controller.DeleteRequest(mirrorsEdgeId, followingId);

            it["is no longer requested"] = () => IsRequested(mirrorsEdgeId, followingId).should_be_false();

            it["other requests are unchanged"] = () =>
            {
                IsRequested(mirrorsEdgeId, anotherUserId).should_be_true();

                IsRequested(gearsOfWarId, followingId).should_be_true();
            };
        }

        void retrieving_requested_games()
        {
            before = () => GivenUserHasRequestedGame(userId, fromUser: followingId, game: mirrorsEdgeId);

            it["lists requested games"] = () =>
                RequestedGames().should_contain(s => s.Name == "Mirror's Edge");

            it["gives user associated with game"] = () => 
                RequestedGames().should_contain(s => s.Owner.Handle == "@following");

            context["user has preferred games (games that haven't been requested)"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: gearsOfWarId);

                it["doesn't contain games that haven't been requested yet"] = () => 
                    RequestedGames().should_not_contain(s => s.Name == "Gears of War");
            };
        }
    }
}
