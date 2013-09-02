using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.belongs_to
{
    class belongs_to_conventional_schema : nspec
    {
        Comments comments;

        Seed seed;

        dynamic blogId, commentId, comment;

        Blogs blogs;

        Familys familys;

        Followings followings;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();
        }

        void describe_retrieval_of_belongs_to()
        {
            context["given blogs that have many comments"] = () =>
            {
                before = () =>
                {
                    comments = new Comments();

                    CreateConventionalBlogTable();

                    CreateConventionalCommentTable();

                    blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                    commentId = new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
                };

                VerifyBelongsToRetrieval();
            };
        }

        void describe_cacheing()
        {
            before = () =>
            {
                blogs = new Blogs();

                comments = new Comments();

                CreateConventionalBlogTable();

                CreateConventionalCommentTable();

                blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                commentId = new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
            };

            act = () =>
            {
                comment = comments.Single(commentId);

                comment.Blog();
            };

            context["belongs to props are changed from external source"] = () =>
            {
                act = () => blogs.Update(new { Title = "Other Title" }, blogId as object);

                it["belongs to properties in comments remain unchanged"] = () =>
                    (comment.Blog().Title as string).should_be("Some Blog");

                it["discarded cache updates properties"] = () =>
                    (comment.Blog(discardCache: true).Title as string).should_be("Other Title");
            };
        }

        void multiple_belongs_to_columns_referencing_the_same_table()
        {
            before = () =>
            {
                familys = new Familys();

                followings = new Followings();

                seed.PurgeDb();

                seed.CreateTable("Familys", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                seed.CreateTable("Followings", new dynamic[] 
                { 
                    seed.Id(),
                    new { FamilyId = "int" },
                    new { FollowingId = "int" }
                }).ExecuteNonQuery();

                var userId = new { Name = "jane" }.InsertInto("Familys");

                var userId2 = new { Name = "john" }.InsertInto("Familys");

                new { FamilyId = userId, FollowingId = userId2 }.InsertInto("Followings");
            };

            it["both belongs to references are accessible"] = () =>
            {
                var firstFollowing = followings.All().First();

                (firstFollowing.Family().Name as string).should_be("jane");

                (firstFollowing.Following().Name as string).should_be("john");
            };
        }

        void CreateConventionalBlogTable()
        {
            seed.CreateTable("Blogs", new dynamic[] {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Title = "nvarchar(255)" },
                new { Body = "nvarchar(255)" }
            }).ExecuteNonQuery();
        }

        void CreateConventionalCommentTable()
        {
            seed.CreateTable("Comments", new dynamic[] {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                new { Text = "nvarchar(1000)" }
            }).ExecuteNonQuery();
        }

        void VerifyBelongsToRetrieval()
        {
            context["retrieving a blog associated with a comment"] = () =>
            {
                act = () => comment = comments.Single(commentId);

                it["returns blog associated with comment"] = () =>
                {
                    (comment.Blog().Id as object).should_be(blogId as object);
                };
            };
        }
    }
}
