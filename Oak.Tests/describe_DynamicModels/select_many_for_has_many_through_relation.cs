using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many_for_has_many_through_relation : _dynamic_models
    {
        dynamic game1Id;

        dynamic game2Id;

        dynamic player1Id;

        dynamic player2Id;

        dynamic email1Id;

        IEnumerable<dynamic> selectMany;

        Players players;

        void before_each()
        {
            players = new Players();

            seed.PurgeDb();

            seed.CreateTable("Players", new dynamic[] 
            { 
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.CreateTable("Games", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Title = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.CreateTable("Library", new dynamic[] 
            { 
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { PlayerId = "int" },
                new { GameId = "int" },
            }).ExecuteNonQuery();

            player1Id = new { Name = "Jane" }.InsertInto("Players");

            player2Id = new { Name = "John" }.InsertInto("Players");

            game1Id = new { Title = "Mirror's Edge" }.InsertInto("Games");

            game2Id = new { Title = "Gears of War" }.InsertInto("Games");

            new { PlayerId = player1Id, GameId = game1Id }.InsertInto("Library");

            new { PlayerId = player2Id, GameId = game2Id }.InsertInto("Library");
        }

        void selecting_many_off_of_collection()
        {
            act = () => selectMany = (players.All() as dynamic).Games();

            it["returns all emails for all users"] = () =>
            {
                selectMany.Count().should_be(2);

                var firstGame = selectMany.First();

                (firstGame.Title as string).should_be("Mirror's Edge");
            };
        }

        void selecting_many_from_nested_relation()
        {
            act = () => selectMany = (players.All() as dynamic).Games().Players();

            it["returns all emails for all users"] = () =>
            {
                selectMany.Count().should_be(2);

                var firstGame = selectMany.First();

                (firstGame.Name as string).should_be("Jane");
            };
        }
    }
}
