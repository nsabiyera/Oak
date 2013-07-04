using System;
using Oak;

namespace BorrowedGames.Models
{
    public class PreferredGame : Gemini
    {
        public PreferredGame(dynamic game, bool isFavorited)
            : base(game as object)
        {
            _.IsFavorited = isFavorited;
        }
    }
}
