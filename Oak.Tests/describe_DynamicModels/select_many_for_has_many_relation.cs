using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many_for_has_many_relation : _dynamic_models
    {
        dynamic user1Id;

        dynamic user2Id;

        dynamic email1Id;

        IEnumerable<dynamic> selectMany;

        Users users;

        void before_each()
        {
            users = new Users();

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

            seed.CreateTable("Aliases", new dynamic[] 
            { 
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { EmailId = "int" },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        void selecting_many_off_of_collection()
        {
            before = () =>
            {
                user1Id = new { Name = "Jane" }.InsertInto("Users");

                user2Id = new { Name = "John" }.InsertInto("Users");

                new { UserId = user1Id, Address = "jane@example.com" }.InsertInto("Emails");

                new { UserId = user2Id, Address = "john@example.com" }.InsertInto("Emails");
            };

            act = () => selectMany = (users.All() as dynamic).Emails();

            it["returns all emails for all users"] = () =>
            {
                selectMany.Count().should_be(2);

                var firstEmail = selectMany.First();

                (firstEmail.Address as string).should_be("jane@example.com");
            };
        }

        void select_many_for_collection_with_where_clause()
        {
            context["where clause returns records"] = () =>
            {
                before = () =>
                {
                    user1Id = new { Name = "Jane" }.InsertInto("Users");

                    user2Id = new { Name = "John" }.InsertInto("Users");

                    new { UserId = user1Id, Address = "jane@example.com" }.InsertInto("Emails");

                    new { UserId = user2Id, Address = "john@example.com" }.InsertInto("Emails");
                };

                act = () => selectMany = (users.All(where: "Id = @0", args: new object[] { user2Id }) as dynamic).Emails();

                it["returns all emails for all users"] = () =>
                {
                    selectMany.Count().should_be(1);

                    var firstEmail = selectMany.First();

                    (firstEmail.Address as string).should_be("john@example.com");
                };

                it["links back to the specific user the query originated from"] = () =>
                {
                    var firstEmail = selectMany.First();

                    (firstEmail.User().Name as string).should_be("John");
                };
            };

            context["where clause returns no records"] = () =>
            {
                act = () => selectMany = (users.All(where: "Id = @0", args: new object[] { user1Id }) as dynamic).Emails();

                it["returns an empty list"] = () => selectMany.Count().should_be(0);
            };
        }

        void select_many_from_nested_relations()
        {
            before = () =>
            {
                user1Id = new { Name = "Jane" }.InsertInto("Users");

                email1Id = new { UserId = user1Id, Address = "jane@example.com" }.InsertInto("Emails");

                new { EmailId = email1Id, Name = "Alias1" }.InsertInto("Aliases");

                new { EmailId = email1Id, Name = "Alias2" }.InsertInto("Aliases");
            };

            act = () => selectMany = (users.All() as dynamic).Emails().Aliases();

            it["returns all records from nested relation"] = () => selectMany.Count().should_be(2);
        }
    }
}
