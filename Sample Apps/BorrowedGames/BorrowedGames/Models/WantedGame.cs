using System;
using System.Collections.Generic;
using Oak;

namespace BorrowedGames.Models
{
    public class WantedGame : DynamicModel
    {
        Games games = new Games();

        Users users = new Users();

        public WantedGame(dynamic dto)
        {
            Init(dto);
        }

        public string Name { get { return This().Game().Name; } }

        public string Console { get { return This().Game().Console; } }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);

            yield return new BelongsTo(users) { ForeignKey = "FromUserId" };
        }
    }
}
