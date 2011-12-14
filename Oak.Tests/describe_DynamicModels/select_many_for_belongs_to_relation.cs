using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak.Tests.describe_DynamicModels.Classes;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class select_many_for_has_one : _dynamic_models
    {
        Users users;

        object userId;

        object userId2;

        void before_each()
        {
            users = new Users();

            seed.PurgeDb();

            seed.CreateTable("Users", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Name = "nvarchar(255)" },
            }).ExecuteNonQuery();

            seed.CreateTable("Profiles", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { UserId = "int" },
                new { DisplayName = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        void select_many_off_of_collection()
        {
            before = () =>
            {
                userId = new { Name = "Jane Doe" }.InsertInto("Users");

                userId2 = new { Name = "John Doe" }.InsertInto("Users");

                new { DisplayName = "Jane", UserId = userId }.InsertInto("Profiles");

                new { DisplayName = "John", UserId = userId2 }.InsertInto("Profiles");
            };

            act = () => models = users.All();

            it["returns associated entities as collection"] = () =>
            {
                var users = models.Profiles();

                (users.First().DisplayName as string).should_be("Jane");

                (users.Last().DisplayName as string).should_be("John");
            };
        }
    }

    class select_many_for_belongs_to_relation : _dynamic_models
    {
        Comments comments;

        object postId;

        object postId2;

        void before_each()
        {
            comments = new Comments();

            seed.PurgeDb();

            seed.CreateTable("Posts", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.CreateTable("Comments", new dynamic[] 
            {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { PostId = "int", ForeignKey = "Posts(Id)" },
                new { Body = "nvarchar(255)" },
                new { RequiresApproval = "bit" }
            }).ExecuteNonQuery();
        }

        void select_many_off_of_collection()
        {
            before = () =>
            {
                postId = new { Name = "Some Post" }.InsertInto("Posts");

                postId2 = new { Name = "Another Post" }.InsertInto("Posts");

                new { PostId = postId, Body = "good stuff", RequiresApproval = true }.InsertInto("Comments");

                new { PostId = postId2, Body = "not bad, not bad", RequiresApproval = true }.InsertInto("Comments");
            };

            act = () => models = comments.All("RequiresApproval = @0", args: new object[] { true });

            it["returns associated entities as collection"] = () =>
            {
                var blogs = models.Posts();

                (blogs.First().Comment().Body as string).should_be("good stuff");

                (blogs.Last().Comment().Body as string).should_be("not bad, not bad");
            };
        }
    }
}
