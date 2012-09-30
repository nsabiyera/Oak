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

    public class Games : DynamicRepository
    {
        public Games()
        {
            Projection = d => new Game(d);
        }
    }

    public class GameSchema
    {
        public static void CreateSchema(Seed seed)
        {
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
        }
    }
}
