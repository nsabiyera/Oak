using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Game : DynamicModel
    {
        Library library = new Library();

        Players players = new Players();

        public Game(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(players, through: library);
        }
    }

    public class Games : DynamicRepository
    {
        public Games()
        {
            Projection = d => new Game(d);
        }
    }
}
