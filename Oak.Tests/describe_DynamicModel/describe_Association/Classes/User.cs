using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class GameWithAutoProps : DynamicModel
    {
        public GameWithAutoProps(object dto)
            : base(dto)
        {

        }

        public GameWithAutoProps()
        {

        }

        public int Id { get; set; }
        public string Title { get; set; }
    }

    public class LibraryWithAutoProps : DynamicModel
    {
        public LibraryWithAutoProps(object dto)
            : base(dto)
        {

        }

        public LibraryWithAutoProps()
        {

        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int GameId { get; set; }
    }

    public class FriendsWithAutoProps : DynamicModel
    {
        public FriendsWithAutoProps(object dto)
            : base(dto)
        {

        }

        public FriendsWithAutoProps()
        {

        }

        public int Id { get; set; }
        public int UserId { get; set; }
        public int IsFollowing { get; set; }
    }

    public class UserWithAutoProps : DynamicModel
    {
        Games games = new Games();

        Library library = new Library();

        Users users = new Users();

        Friends friends = new Friends();

        public UserWithAutoProps() : this(new { }) { }

        public UserWithAutoProps(object entity)
            : base(entity)
        {
            games.Projection = d => new GameWithAutoProps(d).InitializeExtensions();

            library.Projection = d => new LibraryWithAutoProps(d).InitializeExtensions();

            users.Projection = d => new UserWithAutoProps(d).InitializeExtensions();

            friends.Projection = d => new FriendsWithAutoProps(d).InitializeExtensions();
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(games, library)
            {
                XRefFromColumn = "UserId"
            };

            yield return new HasManyThrough(users, friends, methodName: "Friends")
            {
                XRefToColumn = "IsFollowing",
                XRefFromColumn = "UserId"
            };

            yield return new HasMany(library)
            {
                ForeignKey = "UserId"
            };
        }

        public int Id { get; set; }
        public string Email { get; set; }
    }

    public class User : DynamicModel
    {
        Games games = new Games();

        Library library = new Library();

        Users users = new Users();

        Friends friends = new Friends();

        public User() : this(new { }) { }

        public User(object entity)
            : base(entity)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasManyThrough(games, library);

            yield return new HasManyThrough(users, friends, methodName: "Friends")
            {
                XRefToColumn = "IsFollowing"
            };

            yield return new HasMany(library);
        }
    }
}
