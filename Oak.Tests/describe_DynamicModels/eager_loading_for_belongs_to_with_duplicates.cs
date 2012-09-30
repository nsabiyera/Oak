using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_belongs_to_with_duplicates : nspec
    {
        Players players;

        Seed seed;

        dynamic allPlayers;

        object player1Id, player2Id, game1Id, game2Id;

        List<string> sqlQueries;

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

            new { PlayerId = player2Id, GameId = game2Id }.InsertInto("Library");

            sqlQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = new Action<object, string, object[]>(
                (sender, sql, @params) =>
                {
                    sqlQueries.Add(sql);
                });
        }

        [Tag("wip")]
        void belongs_where_entity_belongs_more_than_one_relation()
        {
            before = () =>
            {
                allPlayers = players.All();

                allPlayers.Include("Library");

                allPlayers.Library().Include("Game");
            };

            it["updates all references that map to the belongs to entity"] = () =>
            {
                (allPlayers.First().Library().First().Game().Title as object).should_be("Gears of War");

                (allPlayers.Last().Library().First().Game().Title as object).should_be("Mirror's Edge");

                (allPlayers.Last().Library().Last().Game().Title as object).should_be("Gears of War");

                sqlQueries.Count.should_be(3);
            };
        }
    }
}
