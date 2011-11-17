using System;
using NSpec;
using BorrowedGames.Models;
using Oak;
using Oak.Controllers;
using BorrowedGames.Tests.Controllers;
using System.Collections.Generic;
using System.Linq;

namespace BorrowedGames.Tests.Models
{
    class describe_User : _borrowed_games
    {
        SeedController seed;

        dynamic mirrorsEdgeId, gearsOfWarId, user, userId, followingId, anotherUserId, result;
          
        bool isValid;

        void before_each()
        {
            seed = new SeedController();

            user = ValidUser();

            seed.PurgeDb();

            seed.All();

            userId = GivenUser("user@example.com");

            followingId = GivenUser("following@example.com", "@following");

            anotherUserId = GivenUser("another@example.com", "@another");

            mirrorsEdgeId = GivenGame("Mirror's Edge");

            gearsOfWarId = GivenGame("Gears of War");
        }

        void user_validation()
        {
            act = () => isValid = user.IsValid();

            context["valid user"] = () =>
            {
                before = () => user = ValidUser();

                it["is valid"] = () => isValid.should_be_true();
            };

            context["validating handle"] = () =>
            {
                context["handle doesn't start with @"] = () =>
                {
                    before = () => user.Handle = "somehandle";

                    it["states error"] = () => (user.FirstError() as string).should_be("Your handle has to start with an '@' sign.");

                    it["is invalid"] = () => isValid.should_be_false();
                };

                context["handle contains spaces"] = () =>
                {
                    before = () => user.Handle = "@some handle";

                    it["states error"] = () => (user.FirstError() as string).should_be("Your handle can't contain any spaces.");

                    it["is invalid"] = () => isValid.should_be_false();
                };

                context["handle is @nameless"] = () =>
                {
                    before = () => user.Handle = "@nameless";

                    it["states error"] = () => (user.FirstError() as string).should_be("You did not specify a handle.");

                    it["is invalid"] = () => isValid.should_be_false();
                };

                context["handle not specified"] = () =>
                {
                    before = () => user.Handle = "@";

                    it["states error"] = () => (user.FirstError() as string).should_be("You did not specify a handle.");

                    it["is invalid"] = () => isValid.should_be_false();
                };

                context["handle contains non standard characters"] = () =>
                {
                    before = () => user.Handle = "@hello<script/>";

                    it["states error"] = () => (user.FirstError() as string).should_be("Your handle can only be alpha numeric.");

                    it["is invalid"] = () => isValid.should_be_false();
                };

                context["user selects handle that is taken"] = () =>
                {
                    before = () =>
                    {
                        new { Email = "user@example.com", Handle = "@taken" }.InsertInto("Users");
                        user.Handle = "@taken";
                    };

                    it["states error"] = () => (user.FirstError() as string).should_be("The handle is already taken.");

                    it["is invalid"] = () => isValid.should_be_false();
                };
            };
        }

        void retrieving_preferred_games()
        {
            act = () => result = Users.Single(userId).PreferredGames();

            context["friends have different games that are preferred by user"] = () =>
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
            };

            context["the same preferred games exist across multiple friends"] = () =>
            {
                before = () =>
                {
                    GivenUserHasFriendWithGame(userId, 
                        isFollowing: followingId, 
                        whoHasGame: mirrorsEdgeId);

                    GivenUserHasFriendWithGame(userId,
                        isFollowing: anotherUserId,
                        whoHasGame: mirrorsEdgeId);
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
            };

            context["both requested and unrequested games exist"] = () =>
            {
                before = () =>
                {
                    user = Users.Single(userId);

                    GivenUserIsFollowingTwoPeopleWhoHaveDifferentGames();

                    user.RequestGame(gearsOfWarId, anotherUserId);
                };

                it["requested games show up first"] = () =>
                {
                    ((string)FirstPreferredGame().Name).should_be("Gears of War");
                };
            };
        }

        void GivenUserIsFollowingTwoPeopleWhoHaveDifferentGames()
        {
            GivenUserHasFriendWithGame(userId,
                isFollowing: followingId,
                whoHasGame: mirrorsEdgeId);

            GivenUserHasFriendWithGame(userId,
                isFollowing: anotherUserId,
                whoHasGame: gearsOfWarId);
        }

        dynamic ValidUser()
        {
            return new User(new { Handle = "@amirrajan" });
        }

        dynamic FirstPreferredGame()
        {
            return PreferredGames().First();
        }

        IEnumerable<dynamic> PreferredGames()
        {
            return (result as IEnumerable<dynamic>);
        }

        dynamic OwnerId(dynamic game)
        {
            return Owner(game).Id;
        }

        dynamic Owner(dynamic game)
        {
            return game.Owner;
        }
    }
}
