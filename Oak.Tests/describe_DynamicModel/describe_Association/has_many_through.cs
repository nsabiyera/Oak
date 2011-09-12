using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    public class Library : DynamicRepository { }

    public class Games : DynamicRepository { }

    public class Users : DynamicRepository
    {
        public Users()
        {
            Projection = d => new User(d);
        }
    }

    public class User : DynamicModel
    {
        Games games;

        Library library;

        public User(dynamic entity)
        {
            games = new Games();

            library = new Library();

            Associations(new HasMany(games) { Through = library });

            Init(entity);
        }
    }

    class has_many_through : nspec
    {
        Seed seed;

        IEnumerable<dynamic> games;

        dynamic user;

        Users users;

        void before_each()
        {
            seed = new Seed();

            users = new Users();
        }

        void describe_has_many_through()
        {
            context["given users have a library of games (user has games through library)"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Users", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Email = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Games", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Title = "nvarchar(255)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Library", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { UserId = "int", ForeignKey = "Users(Id)" },
                        new { GameId = "int", ForeignKey = "Games(Id)" },
                    }).ExecuteNonQuery();

                    user = new { Email = "user@example.com" }.InsertInto("Users");

                    var game = new { Title = "Gears of War" }.InsertInto("Games");

                    new { UserId = user, GameId = game }.InsertInto("Library");
                };

                context["retriving games for user's library"] = () =>
                {
                    act = () => games = users.Single(user).Games();

                    it["contains game for user"] = () =>
                        (games.First().Title as string).should_be("Gears of War");
                };
            };
        }
    }
}
