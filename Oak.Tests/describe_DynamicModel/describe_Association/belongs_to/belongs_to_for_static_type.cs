using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.belongs_to
{
    class belongs_to_for_static_type : nspec
    {
        Comments comments;

        Seed seed;

        dynamic blogId, commentId, comment;

        Blogs blogs;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();

            blogs = new Blogs();

            blogs.Projection = d => new BlogWithAutoProps(d).InitializeExtensions();

            comments = new Comments();

            (comments as Comments).Projection = d => new CommentWithAutoProps(d).InitializeExtensions();

            CreateBlogTable();

            CreateCommentTable();

            blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

            commentId = new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
        }

        void retrieving_a_blog_associated_with_a_comment()
        {
            act = () => comment = comments.Single(commentId);

            it["returns blog associated with comment"] = () =>
            {
                (comment.Blog().Id as object).should_be(blogId as object);
            };
        }

        void CreateBlogTable()
        {
            seed.CreateTable("Blogs", new dynamic[] {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { Title = "nvarchar(255)" },
                new { Body = "nvarchar(max)" }
            }).ExecuteNonQuery();
        }

        void CreateCommentTable()
        {
            seed.CreateTable("Comments", new dynamic[] {
                new { Id = "int", Identity = true, PrimaryKey = true },
                new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                new { Text = "nvarchar(1000)" }
            }).ExecuteNonQuery();
        }
    }
}