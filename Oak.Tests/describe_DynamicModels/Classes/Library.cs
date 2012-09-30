using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
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

    public class Library : DynamicRepository
    {
        public Library()
        {
            Projection = d => new LibraryEntry(d);
        }
    }
}
