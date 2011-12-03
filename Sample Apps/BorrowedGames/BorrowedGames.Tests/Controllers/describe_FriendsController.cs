using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Controllers;
using System.Web.Mvc;

namespace BorrowedGames.Tests.Controllers
{
    class describe_FriendsController : _borrowed_games
    {
        FriendsController controller;

        void before_each()
        {
            controller = new FriendsController();

            MockSession(controller);

            controller.CurrentUser = GivenUser("user@example.com", "@me");
        }

        [Tag("describe_User")]
        void when_following_someone()
        {
            act = () => result = controller.Follow("@you");

            context["other users exists"] = () =>
            {
                before = () =>
                {
                    var you = GivenUser("another@example.com", "@you");
                    var existingFriend = GivenUser("friendswithanother@example.com", "@other");
                    GivenUserIsFollowing(controller.CurrentUser, existingFriend);
                };

                it["contains new friend"] = () => Friends().should_contain("@you");

                it["user is notified that friend has been followed"] = () =>
                    (result.Data.Message as string).should_be("You are now following @you.");

                it["added flag is set to true"] = () =>
                    ((bool)result.Data.Added).should_be_true();

                context["subsequent add of the same user"] = () =>
                {
                    act = () => result = controller.Follow("@you");

                    it["disregards add"] = () =>
                    {
                        (result.Data.Message as string).should_be("You are already following @you.");

                        ((bool)result.Data.Added).should_be_false();
                    };
                };
            };

            context["user doesn't exist"] = () =>
            {
                it["doesn't add friends"] = () => Friends().should_be_empty();

                it["user is notified that handle doesn't exist"] = () =>
                    (result.Data.Message as string).should_be("User with handle @you doesn't exist.");

                it["added flag is set to false"] = () =>
                    ((bool)result.Data.Added).should_be_false();
            };
        }

        [Tag("describe_User")]
        void when_unfollowing_someone()
        {
            act = () => result = controller.Unfollow("@you");

            context["user is currently following person"] = () =>
            {
                before = () =>
                {
                    GivenUser("another@example.com", "@you");
                    controller.Follow("@you");
                };

                it["user no longer is following person"] = () => Friends().Count().should_be(0);

                context["subsequent request to remove user"] = () =>
                {
                    before = () => result = controller.Unfollow("@you");

                    it["is ignored"] = () => Friends().Count().should_be(0);
                };
            };

            context["user requesting to be removed doesn't exist"] = () =>
            {
                it["ignores request"] = () => Friends().Count().should_be(0);
            };

            it["returns http 200"] = () => (result as object).should_cast_to<EmptyResult>();
        }

        private IEnumerable<string> Friends()
        {
            return controller.List().Data;
        }
    }
}
