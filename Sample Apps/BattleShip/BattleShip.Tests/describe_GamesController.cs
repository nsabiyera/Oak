using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using BattleShip.Controllers;
using Oak.Controllers;
using Oak;
using System.Web.Mvc;

namespace BattleShip.Tests
{
    [Tag("describe_Game")]
    class describe_GamesController : nspec
    {
        RedirectToRouteResult result;

        int gameId;

        string player1, player2;

        GamesController controller;

        SeedController seedController;

        void before_each()
        {
            controller = new GamesController();

            seedController = new SeedController();

            seedController.PurgeDb();

            seedController.All();
        }

        void creating_a_game()
        {
            act = () =>
            {
                result = GivenAGame(name: "join now");

                gameId = result.GameId();

                player1 = result.UserId();
            };

            it["the game is created"] = () =>
                (GetGame(gameId).Name as object).should_be("join now");

            it["the user who created the game, has joined the game as player 1"] = () =>
            {
                dynamic game = GetGame(gameId);

                (game.Player1Id as object).should_be(player1);

                (game.CurrentTurn as object).should_be(player1);
            };

            it["the game is listed as a game that's available"] = () =>
                (AvailableGames().First().Id as object).should_be(gameId);
        }

        void joining_a_game_that_has_been_created()
        {
            before = () => gameId = GivenAGame(name: "join me").GameId();

            act = () => JoinGame(gameId, userId: Guid.NewGuid().ToString());

            it["the game is not listed as an available game"] = () =>
                AvailableGames().Count().should_be(0);
        }

        void setting_up_a_game()
        {
            context["a game has been created and two people have joined"] = () =>
            {
                before = () =>
                {
                    result = GivenAGame(name: "join now");

                    gameId = result.GameId();

                    player1 = result.UserId();

                    player2 = Guid.NewGuid().ToString();

                    JoinGame(gameId, player2);
                };

                it["squares can be added and removed"] = () =>
                {
                    SetupGame(player1, gameId, "1A");

                    (GetGame(gameId).Player1Squares[0].Location as string).should_be("1A");

                    SetupGame(player1, gameId, "1A");

                    ((int)GetGame(gameId).Player1Squares.Count).should_be(0);
                };

                it["maximum of 5 squares can be marked for each player"] = () =>
                {
                    new string[] 
                    { 
                        "1A", "2A", 
                        "3A", "4A", 
                        "5A", "1B"
                    }.ForEach(location => SetupGame(player1, gameId, location));

                    ((int)GetGame(gameId).Player1Squares.Count).should_be(5);

                    new string[] 
                    { 
                        "1E", "2E", 
                        "3E", "4E", 
                        "5E", "1D"
                    }.ForEach(location => SetupGame(player2, gameId, location));

                    ((int)GetGame(gameId).Player2Squares.Count).should_be(5);
                };
            };
        }

        void ready_to_play()
        {
            before = () =>
            {
                result = GivenAGame(name: "join now");

                gameId = result.GameId();

                player1 = result.UserId();

                player2 = Guid.NewGuid().ToString();

                JoinGame(gameId, player2);

                SetupGame(player1, gameId, "1A");

                SetupGame(player2, gameId, "1B");
            };

            act = () =>
            {
                controller.Ready(new { gameId, playerId = player1 }.ToParams());

                controller.Ready(new { gameId, playerId = player2 }.ToParams());
            };

            it["the game is considered started"] = () =>
                ((bool)GetGame(gameId).Started).should_be_true();

            it["a loser hasn't been assigned"] = () =>
                (GetGame(gameId).Loser as string).should_be("");

            context["player 1 hits"] = () =>
            {
                act = () =>
                    controller.Attack(new
                    {
                        gameId,
                        playerId = player1,
                        location = "1B"
                    }.ToParams());

                it["the hit is marked"] = () =>
                    ((bool)GetGame(gameId).Player1HitsOnPlayer2.Contains("1B")).should_be_true();

                it["the loser is set"] = () =>
                    ((string)GetGame(gameId).Loser).should_be(player2);
            };

            context["player 1 misses"] = () =>
            {
                act = () =>
                    controller.Attack(new
                    {
                        gameId,
                        playerId = player1,
                        location = "2A"
                    }.ToParams());

                it["the miss is marked"] = () =>
                    ((bool)GetGame(gameId).Player1MissesOnPlayer2.Contains("2A")).should_be_true();

                it["is the next player's turn"] = () =>
                    ((string)GetGame(gameId).CurrentTurn).should_be(player2);

                it["attempts to attack by player 1 are ignored"] = () =>
                {
                    controller.Attack(new
                    {
                        gameId,
                        playerId = player1,
                        location = "3A"
                    }.ToParams());

                    ((bool)GetGame(gameId).Player1MissesOnPlayer2.Contains("3A")).should_be_false();
                };

                context["player 2 misses"] = () =>
                {
                    act = () =>
                        controller.Attack(new
                        {
                            gameId,
                            playerId = player2,
                            location = "5A"
                        }.ToParams());

                    it["the miss is marked"] = () =>
                        ((bool)GetGame(gameId).Player2MissesOnPlayer1.Contains("5A")).should_be_true();

                    it["is the next player's turn"] = () =>
                        ((string)GetGame(gameId).CurrentTurn).should_be(player1);
                };

                context["player 2 hits"] = () =>
                {
                    act = () =>
                        controller.Attack(new
                        {
                            gameId,
                            playerId = player2,
                            location = "1A"
                        }.ToParams());

                    it["the hit is marked"] = () =>
                        ((bool)GetGame(gameId).Player2HitsOnPlayer1.Contains("1A")).should_be_true();

                    it["the loser is set"] = () =>
                        ((string)GetGame(gameId).Loser).should_be(player1);
                };
            };
        }

        RedirectToRouteResult GivenAGame(string name)
        {
            return controller.Create(new { name }.ToParams()) as RedirectToRouteResult;
        }

        IEnumerable<dynamic> AvailableGames()
        {
            return (controller.List() as DynamicJsonResult).Data as IEnumerable<dynamic>;
        }

        dynamic GetGame(int id)
        {
            return (controller.Get(id) as DynamicJsonResult).Data;
        }

        void JoinGame(int gameId, string userId)
        {
            controller.Join(new { gameId, userId }.ToParams());
        }

        void SetupGame(string playerId, int gameId, string location)
        {
            controller.SetupGame(new { gameId, location, playerId }.ToParams());
        }
    }

    public static class RedirectToRouteExtensions
    {
        public static int GameId(this RedirectToRouteResult result)
        {
            return Convert.ToInt32(result.RouteValues["gameId"]);
        }

        public static string UserId(this RedirectToRouteResult result)
        {
            return result.RouteValues["userId"].ToString();
        }
    }

    public static class ObjectExtensions
    {
        public static object ToParams(this object o)
        {
            return new Gemini(o);
        }
    }
}
