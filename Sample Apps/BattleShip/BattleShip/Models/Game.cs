using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using BattleShip.Repositories;

namespace BattleShip.Models
{
    public class Game : DynamicModel
    {
        GameSquares gameSquares = new GameSquares();

        GameAttacks gameAttacks = new GameAttacks();

        public Game(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(gameSquares);

            yield return new HasMany(gameAttacks);
        }

        dynamic SquaresFor(dynamic playerId)
        {
            return _.GameSquares().Where(new { PlayerId = playerId });
        }
    }
}