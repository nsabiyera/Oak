using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;

namespace BorrowedGames.Tests.Models.describe_User
{
    [Tag("describe_User")]
    class retrieving_preferred_games : _borrowed_games
    {
        public dynamic mirrorsEdgeXBOX360, gearsOfWarXBOX360Id, user, userId, followingId, anotherUserId, gearsOfWarPS3Id;

        void before_each()
        {
            followingId = GivenUser("following@example.com", "@following");

            anotherUserId = GivenUser("another@example.com", "@another");

            mirrorsEdgeXBOX360 = GivenGame("Mirror's Edge");

            gearsOfWarXBOX360Id = GivenGame("Gears of War");

            gearsOfWarPS3Id = GivenGame("Gears of War", "PS3");

            userId = GivenUser("user@example.com");
        }

        void act_each()
        {
            result = Users.Single(userId).PreferredGames();
        }

        void friends_have_different_games_that_are_preferred_by_user()
        {
            before = () => GivenUserIsFollowingTwoPeopleWhoHaveDifferentGames();

            it["contains each preferred game (ordered alphabetically)"] = () =>
            {
                var gearsOfWar = PreferredGames().First();

                (gearsOfWar.Name as string).should_be("Gears of War");

                (OwnerId(gearsOfWar) as object).should_be(anotherUserId as object);

                (Owner(gearsOfWar).Handle as string).should_be("@another");

                var mirrorsEdge = PreferredGames().Last();

                (mirrorsEdge.Name as string).should_be("Mirror's Edge");

                (mirrorsEdge.Console as string).should_be("XBOX360");

                (OwnerId(mirrorsEdge) as object).should_be(followingId as object);

                (Owner(mirrorsEdge).Handle as string).should_be("@following");
            };

            it["doesn't contain the password for the user when serialized"] = () =>
            {
                var mirrorsEdge = PreferredGames().First();

                (DynamicToJson.Convert(Owner(mirrorsEdge) as object) as string).Contains("Password").should_be_false();

                var gearsOfWar = PreferredGames().Last();

                (DynamicToJson.Convert(Owner(gearsOfWar) as object) as string).Contains("Password").should_be_false();
            };
        }

        void both_requested_and_unrequested_games_exist()
        {
            before = () =>
            {
                user = Users.Single(userId);

                GivenUserIsFollowingTwoPeopleWhoHaveDifferentGames();

                user.WantGame(gearsOfWarXBOX360Id, anotherUserId);
            };

            it["requested games do not show up (they are no longer considered preferred)"] = () =>
                PreferredGames().Any(s => s.Name == "Gears of War").should_be_false();
        }

        void the_same_preferred_games_exist_accross_multiple_friends()
        {
            before = () =>
            {
                GivenUserHasFriendWithGame(userId,
                    isFollowing: followingId,
                    whoHasGame: mirrorsEdgeXBOX360);

                GivenUserHasFriendWithGame(userId,
                    isFollowing: anotherUserId,
                    whoHasGame: mirrorsEdgeXBOX360);
            };

            it["contains an entry for each game"] = () =>
                PreferredGames().Count().should_be(2);

            it["each game has a specific user"] = () =>
            {
                var preferredGame = PreferredGames().First();

                ((int)preferredGame.Owner.Id).should_be(followingId as object);

                preferredGame = PreferredGames().Last();

                ((int)preferredGame.Owner.Id).should_be(anotherUserId as object);
            };
        }

        void user_has_games_for_one_console_friend_as_games_for_another_console()
        {
            before = () =>
            {
                GivenUserHasGame(userId, mirrorsEdgeXBOX360);

                GivenUserHasFriendWithGame(userId, isFollowing: followingId, whoHasGame: gearsOfWarPS3Id);
            };

            it["user's prefered games doesn't include the game for the other console"] = () =>
                PreferredGames().should_not_contain(s => s.Id == gearsOfWarPS3Id);
        }

        public void GivenUserIsFollowingTwoPeopleWhoHaveDifferentGames()
        {
            GivenUserHasFriendWithGame(userId,
                isFollowing: followingId,
                whoHasGame: mirrorsEdgeXBOX360);

            GivenUserHasFriendWithGame(userId,
                isFollowing: anotherUserId,
                whoHasGame: gearsOfWarXBOX360Id);
        }

        public dynamic FirstPreferredGame()
        {
            return PreferredGames().First();
        }

        public IEnumerable<dynamic> PreferredGames()
        {
            return (result as IEnumerable<dynamic>);
        }

        public dynamic OwnerId(dynamic game)
        {
            return Owner(game).Id;
        }

        public dynamic Owner(dynamic game)
        {
            return game.Owner;
        }
    }
}
