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

        public override System.Collections.Generic.IDictionary<string, object> GetAttributesToSave(object o)
        {
            return base.GetAttributesToSave(o).Exclude("Name", "Console");
        }
    }
}
