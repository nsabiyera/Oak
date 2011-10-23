using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class has_many_through : nspec
    {
        dynamic gearsOfWarGameId;

        dynamic mirrorsEdgeGameId;

        Seed seed;

        IEnumerable<dynamic> games;

        IEnumerable<dynamic> gamesIds;

        dynamic userId;

        dynamic user;

        dynamic otherUser;

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

                    userId = new { Email = "user@example.com" }.InsertInto("Users");

                    gearsOfWarGameId = new { Title = "Gears of War" }.InsertInto("Games");

                    new { UserId = userId, GameId = gearsOfWarGameId }.InsertInto("Library");
                };

                context["retriving games for user's library"] = () =>
                {
                    act = () => games = users.Single(userId).Games();

                    it["contains game for user"] = () =>
                        (games.First().Title as string).should_be("Gears of War");
                };

                context["cacheing"] = () =>
                {
                    act = () =>
                    {
                        user = users.Single(userId);
                        user.Games();
                    };

                    it["games are cached until cache is discarded"] = () =>
                    {
                        mirrorsEdgeGameId = new { Title = "Mirror's Edge" }.InsertInto("Games");

                        new { UserId = userId, GameId = gearsOfWarGameId }.InsertInto("Library");

                        var cachedCount = (user.Games() as IEnumerable<dynamic>).Count();

                        cachedCount.should_be(1);

                        var newCount = (user.Games(new { discardCache = true }) as IEnumerable<dynamic>).Count();

                        newCount.should_be(2);
                    };
                };

                context["retrieving game ids for user's library"] = () =>
                {
                    act = () => gamesIds = users.Single(userId).GameIds();

                    it["contains game for user"] = () =>
                        (gamesIds.First() as object).should_be(gearsOfWarGameId as object);
                };
            };
        }

        void has_many_naming_customization()
        {
            context["given users have users through friends"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    seed.CreateTable("Users", new dynamic[] 
                    { 
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { Handle = "nvarchar(1000)" }
                    }).ExecuteNonQuery();

                    seed.CreateTable("Friends", new dynamic[] {
                        new { Id = "int", Identity = true, PrimaryKey = true },
                        new { UserId = "int", ForeignKey = "Users(Id)" },
                        new { IsFollowing = "int", ForeignKey = "Users(Id)" }
                    }).ExecuteNonQuery();

                    userId = new { Handle = "@me" }.InsertInto("Users");

                    otherUser = new { Handle = "@you" }.InsertInto("Users");

                    new { UserId = userId, IsFollowing = otherUser }.InsertInto("Friends");
                };

                it["user named (@me) has friend (@you)"] = () =>
                {
                    (FirstFriendOf(userId).Handle as string).should_be("@you");
                };
            };
        }

        public dynamic FirstFriendOf(dynamic userId)
        {
            return (users.Single(userId).Friends() as IEnumerable<dynamic>).First();
        }
    }
}
