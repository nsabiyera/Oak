using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;

namespace BorrowedGames.Models
{
    public class LendedGame : PotentialRental
    {
        public LendedGame(dynamic game)
            : base(game as object)
        { }
    }

    public class RequestedGame : PotentialRental
    {
        public RequestedGame(dynamic game)
            : base(game as object)
        { }
    }
}