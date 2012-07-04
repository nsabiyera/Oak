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
        dynamic game;

        dynamic gearsOfWarGameId;

        dynamic mirrorsEdgeGameId;

        Seed seed;

        IEnumerable<dynamic> userGames;

        IEnumerable<dynamic> gamesIds;

        dynamic userId;

        dynamic user;

        dynamic otherUser;

        Users users;

        Games games;

        Library library;

        void before_each()
        {
            seed = new Seed();

            users = new Users();

            games = new Games();

            library = new Library();
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
                    act = () => userGames = users.Single(userId).Games();

                    it["contains game for user"] = () =>
                        (userGames.First().Title as string).should_be("Gears of War");

                    it["has a reference back to the user the game belongs to"] = () =>
                    {
                        (userGames.First().User.Email as string).should_be("user@example.com");
                    };
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

                        int cachedCount = user.Games().Count();

                        cachedCount.should_be(1);

                        int newCount = user.Games(new { discardCache = true }).Count();

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

        void newing_up_has_many_association()
        {
            before = () => user = new User(new { Id = 100 });

            context["building a game for user"] = () =>
            {
                act = () => game = user.Games().New(new { Title = "Final Fantasy VII" });

                it["creates a game of type defined in projection"] = () =>
                    (game as object).should_cast_to<Game>();

                context["saving newly created game"] = () =>
                {
                    act = () => games.Save(game);

                    it["saves game"] = () => 
                        games
                            .All()
                            .Any(s => s.Title == "Final Fantasy VII")
                            .should_be_true();

                    it["saving game doesn't automatically associate the has many through"] = () =>
                    {
                        (user.Games(
                            new 
                            { 
                                discardCache = true 
                            }) as IEnumerable<dynamic>).Count().should_be(0);
                    };
                };

                context["saving association through library and game"] = () =>
                {
                    act = () =>
                    {
                        user = users.Single(userId);

                        var game = user.Games().New(new { Title = "Final Fantasy VII" });

                        game.Id = games.Insert(game);

                        var libraryEntry = user.Library().New(new { GameId = game.Id });

                        library.Insert(libraryEntry);
                    };

                    it["game is associated with user"] = () =>
                    {
                        var gameLibrary = user.Games(new { discardCache = true });

                        ((int)gameLibrary.Count()).should_be(2);

                        var game = gameLibrary.Last();

                        (game.Title as string).should_be("Final Fantasy VII");
                    };
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

                it["user named (@me) has friend (@you)"] = () => (FirstFriendOf(userId).Handle as string).should_be("@you");
            };
        }

        public dynamic FirstFriendOf(dynamic userId)
        {
            return users.Single(userId).Friends().First();
        }
    }
}
