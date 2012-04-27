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
            it["marks the game as requested"] = () =>
            {
                GivenUserHasFriendWithGame(currentUserId, isFollowing: friendId, whoHasGame: mirrorsEdgeId);

                IsWanted(mirrorsEdgeId, currentUserId).should_be_true();

                controller.WantGame(mirrorsEdgeId, friendId);

                IsWanted(mirrorsEdgeId, friendId).should_be_true();
            };
        }

        void deleting_wanted_game()
        {
            before = () =>
            {
                GivenUserWantsGame(currentUserId, fromUser: friendId, game: gearsOfWarId);

                GivenUserWantsGame(currentUserId, fromUser: anotherFriendId, game: mirrorsEdgeId);

                GivenUserWantsGame(currentUserId, fromUser: friendId, game: mirrorsEdgeId);
            };

            act = () => controller.DeleteWant(mirrorsEdgeId, friendId);

            it["is no longer requested"] = () => IsWanted(mirrorsEdgeId, friendId).should_be_false();

            it["other requests are unchanged"] = () =>
            {
                IsWanted(mirrorsEdgeId, anotherFriendId).should_be_true();

                IsWanted(gearsOfWarId, friendId).should_be_true();
            };
        }

        void retrieving_wanted_games()
        {
            before = () => GivenUserWantsGame(currentUserId, fromUser: friendId, game: mirrorsEdgeId);

            it["lists requested games"] = () =>
                WantedGames().should_contain(s => s.Name == "Mirror's Edge");

            it["gives user associated with game"] = () => 
                WantedGames().should_contain(s => s.Owner.Handle == "@following");

            it["contains hypermedia link to undo the request of the game"] = () =>
                (WantedGames().First().DeleteWant as string).should_be("/Games/DeleteWant?gameId=" + mirrorsEdgeId + "&followingId=" + friendId);

            context["user has preferred games (games that haven't been requested)"] = () =>
            {
                before = () => GivenUserHasFriendWithGame(currentUserId, isFollowing: friendId, whoHasGame: gearsOfWarId);

                it["doesn't contain games that haven't been requested yet"] = () => 
                    WantedGames().should_not_contain(s => s.Name == "Gears of War");
            };
        }

        public IEnumerable<dynamic> WantedGames()
        {
            return controller.Wanted().Data;
        }
    }
}
