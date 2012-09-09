using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using BorrowedGames.Repositories;

namespace BorrowedGames.Models
{
    public class LibraryEntry : DynamicModel
    {
        Games games = new Games();

        public LibraryEntry(object dto)
            : base(dto)
        {

        }

        public LibraryEntry()
        {
            
        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(games);
        }
    }
}