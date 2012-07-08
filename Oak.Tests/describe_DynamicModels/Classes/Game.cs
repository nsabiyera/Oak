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

        public Game(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(players, through: library);
        }
    }
}
