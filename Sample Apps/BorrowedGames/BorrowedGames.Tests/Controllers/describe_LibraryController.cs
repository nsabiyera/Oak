using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BorrowedGames.Controllers;
using NSpec;
using Oak;

namespace BorrowedGames.Tests.Controllers
{
    class describe_LibraryController : _borrowed_games
    {
        LibraryController controller;

        dynamic user, result, anotherUser;

        object finalFantasy7;

        void before_each()
        {
            controller = new LibraryController();

            user = GivenUser("user@example.com");

            MockSession(controller);

            controller.CurrentUser = user;
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

        void adding_games_to_library()
        {
            before = () => finalFantasy7 = GivenGame("Final Fantasy 7");

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
                before = () => GivenUserHasGame(AnotherUser(), finalFantasy7);

                it["game is available to user"] = () => (Library().First().Name as string).should_be("Final Fantasy 7");
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
                    before = () =>
                    {
                        anotherUser = AnotherUser();
                        GivenUserHasGame(anotherUser, finalFantasy7);
                    };

                    it["his library is unaffected"] = () => LibraryFor(anotherUser as object).Count().should_be(1);
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

        private dynamic AnotherUser()
        {
            return GivenUser("anotheruser@example.com");
        }
    }
}
