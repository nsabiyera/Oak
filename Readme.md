##Why Oak?

Oak is about getting things done. It's about maintaining a rapid development cycle. It's about having an immediate and addictive feedback loop in place to keep you in the zone. It's about getting developers on board and getting them up and running fast so that they can start contributing. It's about reducing all aspects of development friction.

##How Oak?
Oak does this by leveraging dynamic constructs in C# 4.0. These constructs make testing frictionless.  Oak maintains a rapid development cycle by making testing easy and by providing an immediate feedack loop using Growl For Windows.  Oak takes advantage of dynamic C# 4.0 and metaprogramming for your data access.  Initial schema creation, seeding sample data, and regenerating database for other developers is provided directly through controller actions that can be leveraged by tests and rake scripts.  Oak is built on top of ASP.NET MVC, so you don't have to relearn anything, just build upon what you already know.

##What Oak?
- MIT License
- Available via Nuget (install-package oak or install-package oak-edge)
- Continous testing provided by [NSpec](http://nspec.org) and [SpecWatchr](http://nspec.org/continuoustesting) (install-package nspec and install-package specwatchr)

Here is a NSpec test for one of the sample apps:

    class describe_AccountController : _borrowed_games
    {
        AccountController controller;
        dynamic user;
        bool authenticated;

        void before_each()
        {
            controller = new AccountController();
            MockSession(controller);
            controller.Authenticate = s => authenticated = true;
        }

        void registering_for_site()
        {
            context["requesting registration page"] = () =>
            {
                act = () => result = controller.Register();
                it["returns view"] = () => (result as object).should_cast_to<ViewResult>();
            };

            context["user registers"] = () =>
            {
                act = () => result = controller.Register(new
                {
                    Email = "user@example.com",
                    Password = "password",
                    PasswordConfirmation = "password"
                });

                it["logs in user"] = () => (result.Url as string).should_be("/");
                it["authenticates user"] = () => authenticated.should_be_true();
                it["sets user in session"] = () => ((decimal)controller.CurrentUser).should_be((decimal)user);

                context["user name is taken"] = () =>
                {
                    before = () => GivenUser("user@example.com");
                    it["return error stating that user name is taken"] = () =>
                        (result.ViewBag.Flash as string).should_be("Email is unavailable.");
                };
            };

            context["registration is invalid"] = () =>
            {
                act = () => result = controller.Register(new { Email = default(string), Password = default(string) });
                it["returns error stating that email is required."] = () => 
                    (result.ViewBag.Flash as string).should_be("Email is required.");
            };
        }
    }

- A Rails inspired implementation of ActiveModel called **DynamicModel**

      public class User : DynamicModel
      {
          Users users = new Users();
          FriendAssociations friendAssociations = new FriendAssociations();
          Games games = new Games();
          Library library = new Library();
          NotInterestedGames notInterestedGames = new NotInterestedGames();
          GameRequests gameRequests = new GameRequests();

          public User(dynamic dto)
          {
              Init(dto);
          }

          public IEnumerable<dynamic> Associates()
          {
              yield return
              new HasManyThrough(users, through: friendAssociations, named: "Friends") { ForeignKey = "IsFollowing" };

              yield return
              new HasManyThrough(games, through: library);

              yield return
              new HasMany(library);

              yield return
              new HasMany(notInterestedGames);

              yield return
              new HasMany(gameRequests);

              yield return
              new HasMany(friendAssociations);
          }

          public IEnumerable<dynamic> Validates()
          {
              yield return
              new Format("Handle") { With = "^@", ErrorMessage = "Your handle has to start with an '@' sign." };

              yield return
              new Format("Handle") { With = "^[\\S]*$", ErrorMessage = "Your handle can't contain any spaces." };

              yield return
              new Exclusion("Handle") { In = new[] { "@nameless" }, ErrorMessage = "You did not specify a handle." };

              yield return
              new Format("Handle") { With = "[a-zA-Z0-9]+", ErrorMessage = "You did not specify a handle." };

              yield return
              new Format("Handle") { With = "^@[a-zA-Z0-9]*$", ErrorMessage = "Your handle can only be alpha numeric." };

              yield return
              new Uniqueness("Handle", users) { ErrorMessage = "The handle is already taken." };
          }


          public IEnumerable<dynamic> PreferredGames()
          {
              var gamesForFriends = This().Friends().Games() as IEnumerable<dynamic>;

              var distinctPreferredGames =
                  gamesForFriends
                      .Where(s => !OwnsGame(s.Id) && PrefersGame(s.Id) && SharesConsole(s.Console))
                      .Select(game => new
                      {
                          game.Id,
                          game.Name,
                          game.Console,
                          Requested = HasGameBeenRequested(game.Id),
                          Owner = game.User().Select("Id", "Handle")
                      })
                      .OrderBy(s => s.Requested ? 0 : 1)
                      .ThenBy(s => s.Name);

              return distinctPreferredGames;
          }

          public void AddFriend(dynamic user)
          {
              friendAssociations.Insert(new { UserId = This().Id, IsFollowing = user.Id });
          }

          public void RemoveFriend(dynamic friend)
          {
              if (friend == null) return;

              var friendAssociation = This().FriendAssociations().First(new { IsFollowing = friend.Id });

              if (friendAssociation == null) return;

              friendAssociations.Delete(friendAssociation.Id);
          }

          public void RequestGame(dynamic gameId, dynamic fromUserId)
          {
              gameRequests.Insert(new { UserId = This().Id, gameId, fromUserId });
          }

          public bool HasGame(dynamic game)
          {
              return This().Games().Any(new { Id = game.Id });
          }

          public bool HasGames()
          {
              return This().Games().Any();
          }

          public bool HasFriends()
          {
              return This().Friends().Any();
          }

          public void MarkGameNotInterested(dynamic gameId)
          {
              notInterestedGames.Insert(new { UserId = This().Id, gameId });
          }

          public bool HasGameBeenRequested(dynamic gameId)
          {
              return This().GameRequests().Any(new { GameId = gameId });
          }

          public bool OwnsGame(dynamic gameId)
          {
              return This().Games().Any(new { Id = gameId });
          }

          public bool PrefersGame(dynamic gameId)
          {
              return !This().NotInterestedGames().Any(new { GameId = gameId });
          }

          private bool SharesConsole(dynamic console)
          {
              return This().Games().Any(new { Console = console }) || !HasGames();
          }
      }

- Schema generation in C# called **Seed**

      [LocalOnly]
      public class SeedController : Controller
      {
          public Seed Seed { get; set; }

          public SeedController()
          {
              Seed = new Seed();
          }

          [HttpPost]
          public ActionResult All()
          {
              CreateUsers();
              CreateGames();
              CreateLibrary();
              CreateFriends();
              CreateNotInterestedGames();
              CreateGameRequests();
              AddConsoleToGames();
              return new EmptyResult();
          }

          private void CreateUsers()
          {
              Seed.CreateTable("Users", new dynamic[] 
              { 
                  Id(),
                  new { Email = "nvarchar(1000)" },
                  new { Password = "nvarchar(100)" },
                  new { Handle = "nvarchar(1000)" }
              }).ExecuteNonQuery();
          }

          private void CreateGames()
          {
              Seed.CreateTable("Games", new dynamic[] {
                  Id(),
                  new { Name = "nvarchar(1000)" }
              }).ExecuteNonQuery();
          }

          private void CreateLibrary()
          {
              Seed.CreateTable("Library", new dynamic[] {
                  Id(),
                  UserId(),
                  GameId(),
              }).ExecuteNonQuery();
          }

          private void CreateFriends()
          {
              Seed.CreateTable("FriendAssociations", new dynamic[] {
                  Id(),
                  UserId(),
                  new { IsFollowing = "int", ForeignKey = "Users(Id)" }
              }).ExecuteNonQuery();
          }

          private void CreateNotInterestedGames()
          {
              Seed.CreateTable("NotInterestedGames", new dynamic[] { 
                  Id(),
                  UserId(),
                  GameId(),
              }).ExecuteNonQuery();
          }

          private void CreateGameRequests()
          {
              Seed.CreateTable("GameRequests", new dynamic[] {
                  Id(),
                  UserId(),
                  GameId(),
                  new { FromUserId = "int", ForeignKey = "Users(Id)" },
              }).ExecuteNonQuery();
          }

          private void AddConsoleToGames()
          {
              Seed.AddColumns("Games", new dynamic[] 
              { 
                  new { Console = "nvarchar(255)" }
              }).ExecuteNonQuery();
          }

          private object Id()
          {
              return new { Id = "int", Identity = true, PrimaryKey = true };
          }

          private object UserId()
          {
              return new { UserId = "int", ForeignKey = "Users(Id)" };
          }

          private object GameId()
          {
              return new { GameId = "int", ForeignKey = "Games(Id)" };
          }

          [HttpPost]
          public ActionResult PurgeDb()
          {
              Seed.PurgeDb();

              return new EmptyResult();
          }
      }

- General dev assistance in creating schema, building, deploying and running tests provided by [rake-dot-net](http://github.com/amirrajan/rake-dot-net) (install-package rake-dot-net)
- A dynamic ModelBinder called **ParamsModelBinder**
- A set of razor templates used to render Html: **DynamicForm** **OakForm.cshtml**
- [Reference Implementations](https://github.com/amirrajan/Oak/tree/master/Sample%20Apps) are included with the source 

Head over to the [wiki](https://github.com/amirrajan/oak/wiki) for additional details.
