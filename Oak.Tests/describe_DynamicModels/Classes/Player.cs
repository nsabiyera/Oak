using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Player : DynamicModel
    {
        Library library = new Library();

        Games games = new Games();

        public Player(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(games, through: library);
        }
    }
}
