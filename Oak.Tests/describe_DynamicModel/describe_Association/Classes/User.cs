using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
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

            yield return new HasManyThrough(users, friends, named: "Friends")
            {
                ForeignKey = "IsFollowing"
            };

            yield return new HasMany(library);
        }
    }
}
