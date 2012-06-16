using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Oak;
using BattleShip.Repositories;

namespace BattleShip.Controllers
{
    [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
    public class GamesController : Controller
    {
        int maxShips = 5;

        Games games = new Games();

        GameSquares gameSquares = new GameSquares();

        GameAttacks gameAttacks = new GameAttacks();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Create()
        {
            return View();
        }

        public ActionResult SetupGame(int gameId, string userId)
        {
            ViewBag.GameId = gameId;

            ViewBag.UserId = userId;

            return View();
        }

        [HttpPost]
        public ActionResult SetupGame(dynamic @params)
        {
            var game = games.Single(@params.GameId);

            var squareToInsert = new { Location = @params.Location, PlayerId = @params.PlayerId.ToString() };

            var existingSquare = game.GameSquares().First(squareToInsert);

            if (existingSquare != null) gameSquares.Delete(existingSquare.Id);

            else
            {
                var playerSquares = game.GameSquares().Where(new { PlayerId = @params.PlayerId.ToString() }) as IEnumerable<dynamic>;

                if (playerSquares.Count() == maxShips) return new DynamicJsonResult(new { Success = false });

                var gameSquare = game.GameSquares().New(squareToInsert);

                gameSquares.Insert(gameSquare);
            }

            return new DynamicJsonResult(new { Success = true });
        }

        [HttpPost]
        public void Ready(dynamic @params)
        {
            var game = games.Single(@params.GameId);

            if (@params.PlayerId.ToString() == game.Player1Id) game.Player1Ready = true;

            if (@params.PlayerId.ToString() == game.Player2Id) game.Player2Ready = true;

            games.Save(game);
        }

        [HttpPost]
        public ActionResult Create(dynamic @params)
        {
            if (string.IsNullOrEmpty(@params.name)) return View();

            var userId = Guid.NewGuid().ToString();

            @params.CurrentTurn = userId;

            var gameId = Convert.ToInt32(games.Insert(@params));

            Join(new { GameId = gameId, UserId = userId });

            return RedirectToAction("SetupGame", "Games", new { gameId = gameId, userId = userId });
        }

        [HttpPost]
        public ActionResult CreateGameForPhone(dynamic @params)
        {
            var userId = @params.UserId;

            var game = new { Name = @params.Name, CurrentTurn = userId };

            var gameId = Convert.ToInt32(games.Insert(game));

            Join(new { GameId = gameId, UserId = userId });

            return Get(gameId);
        }

        public ActionResult Get(int gameId)
        {
            var game = games.Single(gameId);

            dynamic gameResult = new Gemini();

            gameResult.GameId = game.Id;

            gameResult.Player1Id = game.Player1Id;

            gameResult.Player2Id = game.Player2Id;

            gameResult.Player1Squares = new List<dynamic>();

            gameResult.Player2Squares = new List<dynamic>();

            gameResult.Player2HitsOnPlayer1 = new List<string>();

            gameResult.Player2MissesOnPlayer1 = new List<string>();

            gameResult.Player1HitsOnPlayer2 = new List<string>();

            gameResult.Player1MissesOnPlayer2 = new List<string>();

            gameResult.Player1Ready = game.Player1Ready;

            gameResult.Player2Ready = game.Player2Ready;

            gameResult.Started = false;

            gameResult.CurrentTurn = game.CurrentTurn;

            if (GameStarted(gameResult)) gameResult.Started = true;

            var gameSquares = game.GameSquares();

            foreach (var square in gameSquares)
            {
                if (square.PlayerId == gameResult.Player1Id) gameResult.Player1Squares.Add(square);

                else gameResult.Player2Squares.Add(square);
            }

            var attacks = game.GameAttacks() as IEnumerable<dynamic>;

            gameResult.Loser = "";

            foreach (var attack in attacks)
            {
                var hits = gameResult.Player1HitsOnPlayer2;

                var misses = gameResult.Player1MissesOnPlayer2;

                if (attack.Target == game.Player1Id)
                {
                    hits = gameResult.Player2HitsOnPlayer1;

                    misses = gameResult.Player2MissesOnPlayer1;
                }

                var wasHit = gameSquares.Any(new { PlayerId = attack.Target, Location = attack.Location });

                if (wasHit) hits.Add(attack.Location);

                else misses.Add(attack.Location);
            }

            if (gameResult.Started)
            {
                if (gameResult.Player1HitsOnPlayer2.Count == gameResult.Player2Squares.Count)
                {
                    gameResult.Loser = game.Player2Id;
                }

                if (gameResult.Player2HitsOnPlayer1.Count == gameResult.Player1Squares.Count)
                {
                    gameResult.Loser = game.Player1Id;
                }
            }

            return new DynamicJsonResult(gameResult);
        }

        public ActionResult Play(int gameId, string playerId)
        {
            ViewBag.GameId = gameId;

            ViewBag.PlayerId = playerId;

            return View();
        }

        public ActionResult List()
        {
            return new DynamicJsonResult(OpenGames());
        }

        public dynamic OpenGames()
        {
            return games.All(where: "Player1Id is null or Player2Id is null");
        }

        [HttpPost]
        public ActionResult Join(dynamic @params)
        {
            var game = games.Single(@params.GameId);

            if (string.IsNullOrEmpty(game.Player1Id)) game.Player1Id = @params.UserId;

            else if (string.IsNullOrEmpty(game.Player2Id)) game.Player2Id = @params.UserId;

            if (game.HasChanged()) games.Save(game);

            return Get(Convert.ToInt32(@params.GameId));
        }

        private static bool GameStarted(dynamic game)
        {
            return game.Player1Ready && game.Player2Ready;
        }

        [HttpPost]
        public void Attack(dynamic @params)
        {
            var game = games.Single(@params.GameId);

            if (!GameStarted(game)) return;

            var byPlayer = @params.PlayerId.ToString();

            if (game.CurrentTurn != byPlayer) return;

            var target = game.Player1Id;

            if (target == byPlayer) target = game.Player2Id;

            var square = new { Location = @params.Location, Target = target };

            if (game.GameAttacks().Any(square)) return;

            var attack = game.GameAttacks().New(square);

            gameAttacks.Insert(attack);

            game.CurrentTurn = target;

            games.Save(game);
        }
    }
}
