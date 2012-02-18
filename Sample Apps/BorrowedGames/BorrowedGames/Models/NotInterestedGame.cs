using System;
using Oak;
using System.Collections.Generic;

namespace BorrowedGames.Models
{
    public class NotInterestedGame : DynamicModel
    {
        Games games = new Games();

        public NotInterestedGame(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);
        }

        public string Name
        {
            get { return _.Game().Name; }
        }

        public string Console
        {
            get { return _.Game().Console; }
        }
    }
}
