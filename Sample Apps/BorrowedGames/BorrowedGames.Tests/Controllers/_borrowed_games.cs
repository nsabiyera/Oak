using System;
using NSpec;
using Oak;
using Oak.Controllers;
using System.Collections.Generic;
using BorrowedGames.Controllers;
using BorrowedGames.Models;

namespace BorrowedGames.Tests.Controllers
{
    class _borrowed_games : nspec
    {
        SeedController seedController;

        Dictionary<string, dynamic> session;

        Users users;

        public Users Users
        {
            get { return users; }
            set { users = value; }
        }

        void before_each()
        {
            session = new Dictionary<string, dynamic>();
            seedController = new SeedController();
            seedController.PurgeDb();
            seedController.All();
            Users = new Users();
        }

        protected dynamic GivenUser(string email)
        {
            return GivenUser(email, null, null);
        }

        protected dynamic GivenUser(string email, string handle)
        {
            return GivenUser(email, handle, null);
        }

        protected dynamic GivenUser(string email, string handle, string password)
        {
            return Convert.ToInt32(new { email, password, handle }.InsertInto("Users"));
        }

        protected dynamic GivenGame(string name)
        {
            return Convert.ToInt32(new { name }.InsertInto("Games"));
        }

        protected dynamic GetSessionValue(string key)
        {
            if (!session.ContainsKey(key)) session.Add(key, null);

            return session[key];
        }

        protected void SetSessionValue(string key, dynamic value)
        {
            if (!session.ContainsKey(key)) session.Add(key, null);

            session[key] = value;
        }

        protected void MockSession(BaseController controller)
        {
            controller.SetSessionValue = SetSessionValue;

            controller.GetSessionValue = GetSessionValue;
        }

        protected void GivenUserHasGame(dynamic userId, dynamic gameId)
        {
            new { UserId = userId, GameId = gameId }.InsertInto("Library");
        }

        protected void GivenUserHasFriendWithGame(int userId, int isFollowing, int whoHasGame)
        {
            dynamic user = Users.Single(userId);

            user.AddFriend(Users.Single(isFollowing));

            GivenUserHasGame(isFollowing, whoHasGame);
        }
    }
}
