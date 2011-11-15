using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many : _dynamic_models
    {
        dynamic user1Id;

        dynamic user2Id;

        IEnumerable<dynamic> emailsForUsers;

        Users users;

        void before_each()
        {
            users = new Users();
        }

        void selecting_many_off_of_collection()
        {
            before = () =>
            {
                seed.PurgeDb();

                seed.CreateTable("Users", new dynamic[] 
                { 
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.CreateTable("Emails", new dynamic[] 
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { UserId = "int" },
                    new { Address = "nvarchar(255)" }
                }).ExecuteNonQuery();

                user1Id = new { Name = "Jane" }.InsertInto("Users");

                user2Id = new { Name = "John" }.InsertInto("Users");

                new { UserId = user1Id, Address = "jane@example.com" }.InsertInto("Emails");

                new { UserId = user2Id, Address = "john@example.com" }.InsertInto("Emails");
            };

            act = () => emailsForUsers = (users.All() as dynamic).Emails();

            it["returns all emails for all users"] = () =>
            {
                emailsForUsers.Count().should_be(2);

                var firstEmail = emailsForUsers.First();

                (firstEmail.Address as string).should_be("jane@example.com");
            };
        }
    }
}
