using System;
using Oak;
using System.Collections.Generic;

namespace BorrowedGames.Models
{
    public class NotInterestedGame : DynamicModel
    {
        Games games = new Games();

        public NotInterestedGame(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);
        }

        public string Name
        {
            get { return This().Game().Name; }
        }
    }
}
