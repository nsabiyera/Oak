using System;
using System.Collections.Generic;
using Oak;
using BorrowedGames.Repositories;

namespace BorrowedGames.Models
{
    public class WantedGame : DynamicModel
    {
        Games games = new Games();

        Users users = new Users();

        public WantedGame(object dto)
            : base(dto)
        {
        }

        public string Name { get { return _.Game().Name; } }

        public string Console { get { return _.Game().Console; } }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);

            yield return new BelongsTo(users) { IdColumnOfParentTable = "FromUserId" };

            yield return new BelongsTo(users, methodName: "RequestedBy");
        }

        dynamic IsBorrowed()
        {
            return _.ReturnDate != null;
        }
    }
}
