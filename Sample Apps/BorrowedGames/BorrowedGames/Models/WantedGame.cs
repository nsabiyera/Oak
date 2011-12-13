using System;
using System.Collections.Generic;
using Oak;

namespace BorrowedGames.Models
{
    public class WantedGame : DynamicModel
    {
        Games games = new Games();

        public WantedGame(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);
        }
    }
}
