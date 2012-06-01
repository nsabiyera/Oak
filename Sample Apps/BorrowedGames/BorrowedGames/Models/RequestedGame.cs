using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;

namespace BorrowedGames.Models
{
    public class RequestedGame : Rental
    {
        public RequestedGame(dynamic game)
            : base(game as object)
        { }
    }
}