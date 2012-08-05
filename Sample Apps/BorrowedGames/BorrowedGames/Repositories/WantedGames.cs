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

        public override object Insert(object o)
        {
            return base.Insert(ExcludeProps(o));
        }

        public override int Update(object o, object key)
        {
            return base.Update(ExcludeProps(o), key);
        }

        object ExcludeProps(dynamic o)
        {
            return o.Exclude("Name", "Console");
        }
    }
}