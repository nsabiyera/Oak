using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak.Tests.describe_DynamicModels.Classes;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
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

                (blogs.First().Comment.Body as string).should_be("good stuff");

                (blogs.Last().Comment.Body as string).should_be("not bad, not bad");
            };
        }
    }
}
