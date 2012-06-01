using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Controllers;
using Oak;
using System.Web.Mvc;
using BorrowedGames.Models;
using BorrowedGames.Repositories;

namespace BorrowedGames.Tests.Controllers
{
    class describe_HomeController : _borrowed_games
    {
        HomeController controller;

        dynamic user;

        string handle;

        void before_each()
        {
            controller = new HomeController();

            user = GivenUser("user@example.com");

            MockSession(controller);

            SetCurrentUser(controller, user);
        }

        void visiting_home_page()
        {
            act = () => result = controller.Index();

            context["given user has games"] = () =>
            {
                before = () => GivenUserHasGame(user, GivenGame("Mirror's Edge"));

                it["view is notified that user has games"] = () => ((bool)result.ViewBag.HasGames).should_be_true();
            };

            context["given user hasn't provided a handle"] = () =>
            {
                it["user's handle is null"] = () => ((string)result.ViewBag.Handle).should_be(null);
            };

            context["user has a handle specified"] = () =>
            {
                before = () => controller.Handle(new { handle = "@amirrajan" });

                it["returns user's handle"] = () => ((string)result.ViewBag.Handle).should_be("@amirrajan");
            };

            context["user has friends"] = () =>
            {
                before = () =>
                {
                    var friendId = GivenUser("another@example.com");

                    Users users = new Users();

                    users.Single(user).AddFriend(users.Single(friendId));
                };

                it["view is notified that user has friends"] = () => ((bool)result.ViewBag.HasFriends).should_be_true();
            };
        }

        void updating_user_handle()
        {
            before = () => handle = "@amirrajan";

            act = () => result = controller.Handle(new { handle });

            it["updates user's handle if it's valid"] = () => UserHandle().should_be(handle);

            it["returns message stating that handle has been updated"] = () => StatusMessage().should_be("Your handle has been updated to " + handle + ".");

            it["returns updated handle"] = () => StatusHandle().should_be(handle);

            context["invalid handles"] = () =>
            {
                context["handle specified is @nameless"] = () =>
                {
                    before = () => handle = "@nameless";

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error stating that there is a format error"] = () =>
                        StatusMessage().should_be("You did not specify a handle.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);

                    context["handles are case insensitive, if given '@NAMELESS'"] = () =>
                    {
                        before = () => handle = "@NAMELESS";

                        it["doesn't update user's handle"] = () => UserHandle().should_be(null);
                    };
                };

                context["handle is less than 2 characters"] = () =>
                {
                    before = () => handle = "@";

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error"] = () =>
                        StatusMessage().should_be("You did not specify a handle.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);
                };

                context["handle doesn't start with '@'"] = () =>
                {
                    before = () => handle = "Xodiak";

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error"] = () =>
                        StatusMessage().should_be("Your handle has to start with an '@' sign.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);
                };

                context["handle contains spaces"] = () =>
                {
                    before = () => handle = "@amir rajan";

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error"] = () =>
                        StatusMessage().should_be("Your handle can't contain any spaces.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);
                };

                context["handle contains non alpha numeric characters after the '@' sign"] = () =>
                {
                    before = () => handle = "@amir-rajan";

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error"] = () =>
                        StatusMessage().should_be("Your handle can only be alpha numeric.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);
                };

                context["handles must be unique"] = () =>
                {
                    before = () =>
                    {
                        handle = "@taken";
                        GivenUser("anotheruser@example.com", handle);
                    };

                    it["doesn't update user's handle"] = () => UserHandle().should_be(null);

                    it["returns error"] = () =>
                        StatusMessage().should_be("The handle is already taken.");

                    it["returns current handle"] = () => StatusHandle().should_be(null);
                };
            };

            context["updating handle to the same name"] = () =>
            {
                act = () => result = controller.Handle(new { handle });

                it["ignores update"] = () => UserHandle().should_be(handle);

                it["has no errors"] = () => (result.Data.Message as string).should_be("Your handle has been updated to " + handle + ".");
            };
        }

        public string UserHandle()
        {
            return controller.Index().ViewBag.Handle as string;
        }

        public string StatusMessage()
        {
            return result.Data.Message as string;
        }

        public string StatusHandle()
        {
            return result.Data.Handle as string;
        }
    }
}
