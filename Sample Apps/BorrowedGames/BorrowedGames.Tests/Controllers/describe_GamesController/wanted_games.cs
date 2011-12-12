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
    class wanted_games : _games_controller
    {
        void wanting_game()
        {
            before = () =>
            {
                
            };

            //act = () => 

            it["marks the game as requested"] = () =>
            {
                GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: mirrorsEdgeId);

                IsWanted(mirrorsEdgeId, userId).should_be_true();

                controller.WantGame(mirrorsEdgeId, followingId);

                IsWanted(mirrorsEdgeId, followingId).should_be_true();
            };
        }

        void deleting_wanted_game()
        {
            before = () =>
            {
                GivenUserWantsGame(userId, fromUser: followingId, game: gearsOfWarId);

                GivenUserWantsGame(userId, fromUser: anotherUserId, game: mirrorsEdgeId);

                GivenUserWantsGame(userId, fromUser: followingId, game: mirrorsEdgeId);
            };

            act = () => controller.DeleteWant(mirrorsEdgeId, followingId);

            it["is no longer requested"] = () => IsWanted(mirrorsEdgeId, followingId).should_be_false();

            it["other requests are unchanged"] = () =>
            {
                IsWanted(mirrorsEdgeId, anotherUserId).should_be_true();

                IsWanted(gearsOfWarId, followingId).should_be_true();
            };
        }

        void retrieving_requested_games()
        {
            before = () => GivenUserWantsGame(userId, fromUser: followingId, game: mirrorsEdgeId);

            it["lists requested games"] = () =>
                WantedGames().should_contain(s => s.Name == "Mirror's Edge");

            it["gives user associated with game"] = () => 
                WantedGames().should_contain(s => s.Owner.Handle == "@following");

            context["user has preferred games (games that haven't been requested)"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: gearsOfWarId);

                it["doesn't contain games that haven't been requested yet"] = () => 
                    WantedGames().should_not_contain(s => s.Name == "Gears of War");
            };
        }
    }
}
