using System;

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

            Associations(new HasMany(games) { Through = library });

            Associations(new HasMany(users, named: "Friends") 
            {
                Through = friends,
                On = "IsFollowing"
            });

            Init(entity);
        }
    }
}
