using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_has_one_with_duplicates : nspec
    {
        Players players;

        Seed seed;

        object player1Id, player2Id, game1Id, game2Id;

        void before_each()
        {
            seed = new Seed();

            GameSchema.CreateSchema(seed);

            players = new Players();

            player1Id = new { Name = "Jane" }.InsertInto("Players");

            player2Id = new { Name = "John" }.InsertInto("Players");

            game1Id = new { Title = "Mirror's Edge" }.InsertInto("Games");

            game2Id = new { Title = "Gears of War" }.InsertInto("Games");

            new { PlayerId = player1Id, GameId = game2Id }.InsertInto("Library");

            new { PlayerId = player2Id, GameId = game1Id }.InsertInto("Library");
        }

        [Tag("wip")]
        void it_works()
        {

        }

    }
}
