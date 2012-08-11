using System;
using System.Collections.Generic;
using Oak;
using BorrowedGames.Repositories;

namespace BorrowedGames.Models
{
    public class Game : Gemini
    {
        FavoritedGames favoritedGames = new FavoritedGames();

        public Game(object dto)
            : base(dto)
        {
            
        }

        void FavoritedBy(dynamic user)
        {
            favoritedGames.Insert(new { GameId = _.Id, UserId = user.Id });
        }

        void UnfavoritedBy(dynamic user)
        {
            var id = favoritedGames.SingleWhere("GameId = @0 and UserId = @1", new object[] { _.Id, user.Id }).Id;

            favoritedGames.Delete(id);
        }
    }
}
