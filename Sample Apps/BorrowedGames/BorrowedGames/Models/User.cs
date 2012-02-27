using System;
using Oak;
using System.Linq;
using System.Collections.Generic;

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

        public User(object dto)
            : base(dto)
        {
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
            new HasMany(wantedGames, named: "Wants");

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

        public void GameGiven(dynamic gameId, dynamic toUserId)
        {
            var wantedGame = wantedGames.All().First();

            wantedGame.ReturnDate = DateTime.Today.AddMonths(1);

            wantedGames.Update(wantedGame, wantedGame.Id);
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
            return _.Wants().Any(new { GameId = game.Id, FromUserId = game.User().Id });
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
                .Select(UserGame)
                .ToList();
        }

        public IEnumerable<dynamic> RequestedGames()
        {
            return wantedGames.All(where: "FromUserId = @0", args: new object[] { _.Id })
                .Select(RequestedGame)
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
                    .Select(UserGame)
                    .OrderBy(s => s.Name)
                    .ToList();

            return preferredGames;
        }

        IEnumerable<dynamic> BorrowedGames()
        {
            return _.Wants().Where(new { IsBorrowed = true });
        }

        private dynamic UserGame(dynamic game)
        {
            return new Gemini(new
            {
                game.Id,
                game.Name,
                game.Console,
                Owner = game.User().Select("Id", "Handle")
            });
        }

        private dynamic RequestedGame(dynamic game)
        {
            return new Gemini(new
            {
                game.Id,
                game.Name,
                game.Console,
                RequestedBy = game.RequestedBy().Select("Id", "Handle"),
                game.ReturnDate
            });
        }
    }
}
