using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using BorrowedGames.Models;

namespace BorrowedGames.Repositories
{
    public class WantedGames : DynamicRepository
    {
        public WantedGames()
        {
            Projection = d => new WantedGame(d);
        }

        public override dynamic GetAttributesToSave(object o)
        {
            return base.GetAttributesToSave(o).Exclude("Name", "Console");
        }
    }
}