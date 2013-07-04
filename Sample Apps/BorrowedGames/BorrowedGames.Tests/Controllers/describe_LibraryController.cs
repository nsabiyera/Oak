using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BorrowedGames.Controllers;
using NSpec;
using Oak;
using BorrowedGames.Models;

namespace BorrowedGames.Tests.Controllers
{
    class describe_LibraryController : _borrowed_games
    {
        static List<Email> emailsSent;

        LibraryController controller;

        dynamic user, anotherUser, yetAnotherUser;

        object finalFantasy7, finalFantasyTactics;

        static describe_LibraryController()
        {
            Gemini.Initialized<Email>(e =>
            {
                e.Send = new DynamicMethod(() =>
                {
                    emailsSent.Add(e);

                    return null;
                });
            });
        }

        void before_each()
        {
            emailsSent = new List<Email>();

            controller = new LibraryController();

            user = GivenUser("user@example.com", "@user");

            anotherUser = GivenUser("anotheruser@example.com");

            yetAnotherUser = GivenUser("yetanotheruser@example.com");

            SetCurrentUser(controller, user);
        }

        void retrieving_library()
        {
            act = () => result = controller.List();

            context["given user has games"] = () =>
            {
                before = () => GivenUserHasGame(user, GivenGame("Mirror's Edge"));

                it["returns a json payload containing games"] = () => (Games().First().Name as string).should_be("Mirror's Edge");
            };

            context["given user has no games"] = () =>
            {
                it["the json payload is empty"] = () => Games().should_be_empty();
            };
        }

        [Tag("describe_Games")]
        void searching_games()
        {
            act = () => result = controller.Search("Final Fan");

            context["games found that start with search string"] = () =>
            {
                before = () =>
                {
                    finalFantasy7 = GivenGame("Final Fantasy VII");
                    GivenGame("The Final Destination");
                    GivenGame("Mirror's Edge");
                };

                it["contains all games that start with search string"] = () =>
                {
                    GameIdFor("Final Fantasy VII").should_be(finalFantasy7);

                    GameNames().should_contain("Final Fantasy VII");
                    GameNames().should_not_contain("Mirror's Edge");
                    GameNames().should_not_contain("The Final Destination");
                };
            };

            context["no games found that start with search string, but games contain search string"] = () =>
            {
                before = () =>
                {
                    GivenGame("The Final Fanfare Destination");
                    GivenGame("Mirror's Edge");
                };

                it["contains games that contain the search string"] = () =>
                {
                    GameNames().should_contain("The Final Fanfare Destination");
                    GameNames().should_not_contain("Mirror's Edge");
                };
            };

            context["no games found that start with or contain search string"] = () =>
            {
                before = () =>
                {
                    GivenGame("The Final Baseball Fan");
                    GivenGame("Mirror's Edge");
                };

                it["contains games that have the same words, even if they are not side by side"] = () =>
                {
                    GameNames().should_contain("The Final Baseball Fan");
                    GameNames().should_not_contain("Mirror's Edge");
                };
            };
        }

        [Tag("describe_FriendAssociation,describe_User,describe_Email")]
        void adding_games_to_library()
        {
            before = () => finalFantasy7 = GivenGame("Final Fantasy 7", "PS2");

            act = () => result = controller.Add(new { gameId = finalFantasy7 });

            it["game is available to user"] = () => (Library().First().Name as string).should_be("Final Fantasy 7");

            it["returns the game that was added"] = () => (result.Data.Name as string).should_be("Final Fantasy 7");

            context["same game is added to library"] = () =>
            {
                act = () => controller.Add(new { gameId = finalFantasy7 });

                it["the game isn't added"] = () => Library().Count().should_be(1);
            };

            context["game libraries are unique accross users"] = () =>
            {
                before = () => GivenUserHasGame(anotherUser, finalFantasy7);

                it["game is available to user"] = () => (Library().First().Name as string).should_be("Final Fantasy 7");
            };

            context["friends are following the person who added the game"] = () =>
            {
                before = () =>
                {
                    GivenUserIsFollowing(anotherUser, user);
                    GivenUserIsFollowing(yetAnotherUser, user);
                };

                it["notification emails are sent to followers"] = () =>
                {
                    var email = emailsSent[0];

                    email.To.should_be(User(anotherUser).Email as string);

                    var subject = User(user).Handle as string + " added Final Fantasy 7 (PS2) to his library.";

                    email.Subject.should_be(subject);

                    email.Body.should_be(subject + Environment.NewLine + "Go check it out at http://borrowedgames.com/");

                    email = emailsSent[1];

                    email.To.should_be(User(yetAnotherUser).Email as string);

                    emailsSent.Count.should_be(2);
                };

                context["user adds another game the same day", "wip"] = () =>
                {
                    before = () => finalFantasyTactics = GivenGame("Final Fantasy Tactics", "PS2");

                    act = () =>
                    {
                        emailsSent.Count.should_be(2);
                        emailsSent.Clear();
                        controller.Add(new { gameId = finalFantasyTactics });
                    };

                    it["a subsequent email for that day is not sent"] = () =>
                        emailsSent.Count.should_be(0);
                };
            };
        }

        [Tag("describe_User")]
        void removing_game_from_library()
        {
            act = () => controller.Delete(new { gameId = finalFantasy7 });

            context["user has game"] = () =>
            {
                before = () =>
                {
                    finalFantasy7 = GivenGame("Final Fantasy 7");
                    controller.Add(new { gameId = finalFantasy7 });
                };

                it["the game is removed from the library"] = () => Library().Count().should_be(0);

                context["susequently removing the same game"] = () =>
                {
                    act = () => controller.Delete(new { gameId = finalFantasy7 });

                    it["is ignored"] = () => Library().Count().should_be(0);
                };

                context["another user has same game"] = () =>
                {
                    before = () => GivenUserHasGame(anotherUser, finalFantasy7);

                    it["his library is unaffected"] = () => 
                        LibraryFor(anotherUser as object).Count().should_be(1);
                };
            };
        }

        IEnumerable<dynamic> LibraryFor(dynamic user)
        {
            return (controller.ListFor(user).Data as IEnumerable<dynamic>);
        }

        IEnumerable<dynamic> Library()
        {
            return LibraryFor(user);
        }

        IEnumerable<dynamic> Games()
        {
            return (result.Data as IEnumerable<dynamic>);
        }

        List<string> GameNames()
        {
            return Games().Select(s => s.Name as string).ToList();
        }

        private object GameIdFor(string name)
        {
            return Games().Single(s => s.Name == name).Id;
        }
    }
}
