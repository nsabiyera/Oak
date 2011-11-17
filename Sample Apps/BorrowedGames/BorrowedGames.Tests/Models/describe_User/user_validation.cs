using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BorrowedGames.Models;
using Oak;

namespace BorrowedGames.Tests.Models.describe_User
{
    [Tag("describe_User")]
    class user_validation : _borrowed_games
    {
        bool isValid;

        dynamic userId, user;

        void before_each()
        {
            userId = GivenUser("user@example.com");

            user = ValidUser();
        }

        void act_each()
        {
            isValid = user.IsValid();
        }

        void valid_user()
        {
            before = () => user = ValidUser();

            it["is valid"] = () => isValid.should_be_true();
        }

        void validating_handle()
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
        }

        public dynamic ValidUser()
        {
            return new User(new { Handle = "@amirrajan" });
        }
    }
}
