using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

namespace BorrowedGames.Models
{
    public class NotInterestedGames : DynamicRepository
    {
        public NotInterestedGames()
        {
            Projection = d => new NotInterestedGame(d);
        }
    }
}
