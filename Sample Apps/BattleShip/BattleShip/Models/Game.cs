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

        dynamic HitsOn(dynamic playerId)
        {
            var attacks = _.GameAttacks().Where(new { Target = playerId });

            var squares = _.SquaresFor(playerId);

            var hits = new List<dynamic>();

            foreach (var attack in attacks)
            {
                if (squares.Any(new { Location = attack.Location })) hits.Add(attack.Location);
            }

            return hits;
        }

        dynamic MissesOn(dynamic playerId)
        {
            var attacks = _.GameAttacks().Where(new { Target = playerId });

            var squares = _.SquaresFor(playerId);

            var misses = new List<dynamic>();

            foreach (var attack in attacks)
            {
                if (!squares.Any(new { Location = attack.Location })) misses.Add(attack.Location);
            }

            return misses;
        }

        dynamic Started()
        {
            return _.Player1Ready && _.Player2Ready;
        }

        dynamic Loser()
        {
            if (!_.Started()) return "";

            if (AllShipsDestroyedFor(_.Player1Id)) return _.Player1Id;

            else if (AllShipsDestroyedFor(_.Player2Id)) return _.Player2Id;

            return "";
        }

        void Join(dynamic playerId)
        {
            if (string.IsNullOrEmpty(_.Player1Id))
            {
                _.Player1Id = playerId;

                _.CurrentTurn = playerId;
            }

            else if (string.IsNullOrEmpty(_.Player2Id)) _.Player2Id = playerId;   
        }

        dynamic AllShipsDestroyedFor(dynamic playerId)
        {
            return _.HitsOn(playerId).Count == _.SquaresFor(playerId).Count();
        }
    }
}