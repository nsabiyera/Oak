using System;
using Oak;

namespace BorrowedGames.Models
{
    public class PreferredGame : Gemini
    {
        public PreferredGame(dynamic game)
            : base(game as object)
        { }
    }
}
