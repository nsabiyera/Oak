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
                var playerSquares = game.SquaresFor(@params.PlayerId.ToString());

                if (playerSquares.Count() == maxShips) return new DynamicJsonResult(new { Success = false });

                gameSquares.Insert(game.GameSquares().New(squareToInsert));
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

            var gameId = Convert.ToInt32(games.Insert(@params));

            Join(new { GameId = gameId, UserId = userId });

            return RedirectToAction("SetupGame", "Games", new { gameId = gameId, userId = userId });
        }

        public ActionResult Get(int gameId)
        {
            var game = games.Single(gameId);

            dynamic gameResult = new Gemini(game);

            gameResult.Player1Squares = game.SquaresFor(game.Player1Id);

            gameResult.Player2Squares = game.SquaresFor(game.Player2Id);

            gameResult.Player2HitsOnPlayer1 = game.HitsOn(game.Player1Id);

            gameResult.Player2MissesOnPlayer1 = game.MissesOn(game.Player1Id);

            gameResult.Player1HitsOnPlayer2 = game.HitsOn(game.Player2Id);

            gameResult.Player1MissesOnPlayer2 = game.MissesOn(game.Player2Id);

            gameResult.Started = game.Started();

            gameResult.Loser = game.Loser();

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

            game.Join(@params.UserId);

            games.Save(game);

            return Get(Convert.ToInt32(@params.GameId));
        }

        [HttpPost]
        public void Attack(dynamic @params)
        {
            var game = games.Single(@params.GameId);

            if (!game.Started()) return;

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
