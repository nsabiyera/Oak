using System;

namespace BorrowedGames.Models
{
    public class LendedGame : Rental
    {
        public LendedGame(dynamic game)
            : base(game as object)
        { }
    }
}
