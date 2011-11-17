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
            return This().Games().Any(new { Console = console }) || (This().Games() as IEnumerable<dynamic>).Count() == 0;
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
    }
}
