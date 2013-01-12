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

        void belongs_where_entity_belongs_more_than_one_relation()
        {
            before = () =>
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

        dynamic tasks = new Tasks();

        object rabbitId, task1Id, task2Id, task3Id;

        void describe_track_back_property()
        {
            before = () =>
            {
                Seed seed = new Seed();

                seed.PurgeDb();

                seed.CreateTable("Rabbits", seed.Id(), new { Name = "nvarchar(255)" }).ExecuteNonQuery();

                seed.CreateTable("Tasks", seed.Id(), new { Description = "nvarchar(255)" }, new { RabbitId = "int" }).ExecuteNonQuery();

                rabbitId = new { Name = "YT" }.InsertInto("Rabbits");

                task1Id = new { Description = "task 1", rabbitId }.InsertInto("Tasks");

                task2Id = new { Description = "task 2", rabbitId }.InsertInto("Tasks");

                task3Id = new { Description = "task 3", rabbitId }.InsertInto("Tasks");
            };

            it["tracks back to multiple instances, converting a single value to a collection"] = () =>
            {
                var allTasks = tasks.All().Include("Rabbit") as DynamicModels;

                var task = allTasks.First();

                (task.Rabbit().Task as List<dynamic>).Count.should_be(3);

                (task.Rabbit().Task as List<dynamic>).should_contain(allTasks.First() as object);

                (task.Rabbit().Task as List<dynamic>).should_contain(allTasks.Second() as object);

                (task.Rabbit().Task as List<dynamic>).should_contain(allTasks.Last() as object);
            };
        }
    }
}
