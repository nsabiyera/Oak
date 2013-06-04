using System;
using Oak;
using System.Linq;
using System.Collections.Generic;
using BorrowedGames.Repositories;

namespace BorrowedGames.Models
{
    public class User : DynamicModel
    {
        Users users = new Users();

        FriendAssociations friendAssociations = new FriendAssociations();

        Games games = new Games();

        Library library = new Library();

        NotInterestedGames notInterestedGames = new NotInterestedGames();

        WantedGames wantedGames = new WantedGames();

        FavoritedGames favoritedGames = new FavoritedGames();

        public User(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
            new HasManyThrough(users, through: friendAssociations, methodName: "Friends") { XRefToColumn = "IsFollowing" };

            yield return
            new HasManyThrough(games, through: library);

            yield return
            new HasMany(library);

            yield return
            new HasMany(notInterestedGames);

            yield return
            new HasMany(wantedGames, methodName: "Wants");

            yield return
            new HasMany(friendAssociations);

            yield return
            new HasMany(favoritedGames);
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

        public void AddFriend(dynamic user)
        {
            friendAssociations.Insert(_.FriendAssociations().New(new { IsFollowing = user.Id }));
        }

        public void RemoveFriend(dynamic friend)
        {
            if (friend == null) return;

            var friendAssociation = _.FriendAssociations().First(new { IsFollowing = friend.Id });

            if (friendAssociation == null) return;

            friendAssociations.Delete(friendAssociation.Id);
        }

        public void WantGame(dynamic gameId, dynamic fromUserId)
        {
            wantedGames.Insert(_.Wants().New(new { gameId, fromUserId }));
        }

        public void GiveGame(dynamic gameId, dynamic toUserId)
        {
            var wantedGame = WantedGame(gameId, toUserId);

            wantedGame.ReturnDate = DateTime.Today.AddMonths(1);

            wantedGames.Update(wantedGame, wantedGame.Id);
        }

        public dynamic WantedGame(int gameId, int ownerId)
        {
            return wantedGames.SingleWhere("GameId = @0 and UserId = @1", new object[] { gameId, ownerId });
        }

        public void UndoNotInterestedGame(dynamic gameId)
        {
            notInterestedGames.Delete(GameInNotInterestedList(gameId).Id);
        }

        public dynamic GameInNotInterestedList(dynamic gameId)
        {
            return _.NotInterestedGames().First(new { GameId = gameId });
        }

        public void DeleteWantedGame(dynamic gameId, dynamic fromUserId)
        {
            wantedGames.Delete(GameInWantedList(gameId, fromUserId).Id);
        }

        public dynamic GameInWantedList(dynamic gameId, dynamic fromUserId)
        {
            return _.Wants().First(new { GameId = gameId, FromUserId = fromUserId });
        }

        public bool HasGame(dynamic game)
        {
            return _.Games().Any(new { Id = game.Id });
        }

        public bool HasGames()
        {
            return _.Games().Any();
        }

        public bool HasFriends()
        {
            return _.Friends().Any();
        }

        public void MarkGameNotInterested(dynamic gameId)
        {
            notInterestedGames.Insert(_.NotInterestedGames().New(new { gameId }));
        }

        public bool GameIsWanted(dynamic game)
        {
            return _.Wantss().Any(new { GameId = game.Id, FromUserId = game.User.Id });
        }

        public bool HasNotBeenRequested(dynamic game)
        {
            return !GameIsWanted(game);
        }

        public bool OwnsGame(dynamic gameId)
        {
            return _.Games().Any(new { Id = gameId });
        }

        bool DoesNotOwnGame(dynamic gameId)
        {
            return !OwnsGame(gameId);
        }

        public bool PrefersGame(dynamic gameId)
        {
            return !_.NotInterestedGames().Any(new { GameId = gameId });
        }

        private bool SharesConsole(dynamic console)
        {
            return _.Games().Any(new { Console = console }) || !HasGames();
        }
            
        public IEnumerable<dynamic> WantedGames()
        {
            return GamesFriendsHave()
                .Where(GameIsWanted)
                .Select(WantedGame)
                .ToList();
        }

        public IEnumerable<dynamic> RequestedGames()
        {
            return wantedGames
                .All(where: "FromUserId = @0", args: new object[] { _.Id })
                .Select(g => 
                {
                    if (g.ReturnDate == null) return new RequestedGame(g) as dynamic;

                    return new LendedGame(g) as dynamic;
                })
                .ToList();
        }

        public IEnumerable<dynamic> GamesFriendsHave()
        {
            return _.Friends().Games();
        }

        public IEnumerable<dynamic> PreferredGames()
        {
            var preferredGames =
                GamesFriendsHave()
                    .Where(s =>
                        DoesNotOwnGame(s.Id) &&
                        PrefersGame(s.Id) &&
                        SharesConsole(s.Console) &&
                        HasNotBeenRequested(s))
                    .Select(PreferredGame)
                    .OrderByDescending(s => s.IsFavorited)
                    .ThenBy(s => s.Name)
                    .ToModels();

            return preferredGames;
        }

        IEnumerable<dynamic> BorrowedGames()
        {
            return (_.Wants().Where(new { IsBorrowed = true }) as IEnumerable<dynamic>)
                .Select(g => new RequestedGame(g))
                .ToList();
        }

        private dynamic PreferredGame(dynamic game)
        {
            return new PreferredGame(new
            {
                game.Id,
                game.Name,
                game.Console,
                Owner = game.User.Select("Id", "Handle")
            }, _.FavoritedGames().Any(new { gameId = game.Id }));
        }

        private dynamic WantedGame(dynamic game)
        {
            var returnDate = WantedGame(game.Id, _.Id, game.User.Id).ReturnDate;
            var isBorrowed = returnDate != null;
            int? daysLeft = null;
            if(isBorrowed) daysLeft = Convert.ToInt32((returnDate - DateTime.Today).TotalDays); 

            return new Gemini(new
            {
                game.Id,
                game.Name,
                game.Console,
                ReturnDate = returnDate,
                DaysLeft = daysLeft,
                Owner = game.User.Select("Id", "Handle"),
                IsBorrowed = new DynamicFunction(() => isBorrowed)
            });
        }

        public void GameReturned(int gameId, int byUserId)
        {
            var wantedGame = WantedGame(gameId, byUserId);

            wantedGames.Delete(wantedGame.Id);
        }

        public void ReturnGame(int gameId, int toUserId)
        {
            var wantedGame = WantedGame(gameId, _.Id, toUserId);

            wantedGames.Delete(wantedGame.Id);
        }

        public dynamic WantedGame(int gameId, int requesterId, int ownerId)
        {
            return wantedGames.SingleWhere("GameId = @0 and FromUserId = @1 and UserId = @2", new object[] { gameId, ownerId, _.Id });
        }

        dynamic FollowerAssociations()
        {
            return friendAssociations.All(where: "IsFollowing = @0", args: new object[] { _.Id });
        }

        IEnumerable<dynamic> Followers()
        {
            return FollowerAssociations().User();
        }
    }
}
