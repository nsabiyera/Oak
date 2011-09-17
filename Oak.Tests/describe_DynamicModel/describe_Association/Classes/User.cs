using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class User : DynamicModel
    {
        Games games;

        Library library;

        Users users;

        Friends friends;

        public User(dynamic entity)
        {
            games = new Games();

            library = new Library();

            users = new Users();

            friends = new Friends();

            Init(entity);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return 
            new HasMany(games) { Through = library };

            yield return
            new HasMany(users, named: "Friends")
            {
                Through = friends,
                Using = "IsFollowing"
            };
        }
    }
}
