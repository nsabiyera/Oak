using System;
using NSpec;
using Oak;
using Oak.Controllers;
using System.Collections.Generic;
using BorrowedGames.Controllers;
using BorrowedGames.Models;
using System.Web.Routing;
using Moq;
using System.Web;
using System.Web.Mvc;

namespace BorrowedGames.Tests
{
    class _borrowed_games : nspec
    {
        SeedController seedController;

        Dictionary<string, dynamic> session;

        Users users;

        public dynamic result;

        public Users Users
        {
            get { return users; }
            set { users = value; }
        }

        void before_each()
        {
            session = new Dictionary<string, dynamic>();
            seedController = new SeedController();
            seedController.DeleteAllRecords();
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

        protected dynamic GivenGame(string name, string console = "XBOX360")
        {
            return Convert.ToInt32(new { name, console }.InsertInto("Games"));
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

        public void MockRouting(BaseController controller)
        {
            var routes = new RouteCollection();
            MvcApplication.RegisterRoutes(routes);

            var request = new Mock<HttpRequestBase>(MockBehavior.Strict);
            request.SetupGet(x => x.ApplicationPath).Returns("/");
            request.SetupGet(x => x.Url).Returns(new Uri("", UriKind.Relative));
            request.SetupGet(x => x.ServerVariables).Returns(new System.Collections.Specialized.NameValueCollection());

            var response = new Mock<HttpResponseBase>();
            response.Setup(x => x.ApplyAppPathModifier(It.IsAny<string>())).Returns<string>((s) => s);

            var context = new Mock<HttpContextBase>(MockBehavior.Strict);
            context.SetupGet(x => x.Request).Returns(request.Object);
            context.SetupGet(x => x.Response).Returns(response.Object);

            controller.Url = new UrlHelper(new RequestContext(context.Object, new RouteData()), routes);
        }

        protected void GivenUserHasGame(dynamic userId, dynamic gameId)
        {
            new { UserId = userId, GameId = gameId }.InsertInto("Library");
        }

        protected void GivenUserHasFriendWithGame(int userId, int isFollowing, int whoHasGame)
        {
            GivenUserIsFollowing(userId, isFollowing);

            GivenUserHasGame(isFollowing, whoHasGame);
        }

        protected void GivenUserWantsGame(int userId, int fromUser, int game)
        {
            GivenUserHasFriendWithGame(userId, fromUser, game);

            User(userId).WantGame(game, fromUser);
        }

        protected dynamic User(int userId)
        {
            return Users.Single(userId);
        }

        protected void GivenUserIsFollowing(int userId, int isFollowing)
        {
            dynamic user = Users.Single(userId);

            user.AddFriend(Users.Single(isFollowing));
        }

        protected DateTime OneMonthFromToday()
        {
            return DateTime.Today.AddMonths(1);
        }
    }
}
