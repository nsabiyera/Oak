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
    }
}
