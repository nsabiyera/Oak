using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;
using BorrowedGames.Models;

namespace BorrowedGames.Repositories
{
    public class Library : DynamicRepository
    {
        public Library()
        {
            Projection = d => new LibraryEntry(d);
        }
    }
}