using System;
using Massive;
using BorrowedGames.Models;

namespace BorrowedGames.Repositories
{
    public class NotInterestedGames : DynamicRepository
    {
        public NotInterestedGames()
        {
            Projection = d => new NotInterestedGame(d);
        }

        public override object Insert(dynamic o)
        {
            return base.Insert(o.Exclude("Name", "Console") as object);
        }
    }
}
