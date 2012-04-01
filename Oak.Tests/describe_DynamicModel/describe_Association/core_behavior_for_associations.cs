using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class core_behavior_for_associations : nspec
    {
        class Comments : DynamicRepository
        {
            public Comments()
            {
                Projection = d => new Comment(d);
            }
        }

        class Blogs : DynamicRepository
        {

        }

        class Comment : Gemini
        {
            static Comment()
            {
                Gemini.Initialized<Comment>(i =>
                {
                    i.Associates = new DynamicFunction(() =>
                    {
                        return new[] 
                        {
                            new BelongsTo(i.Blogs)
                        };
                    });

                    new Associations(i);
                });
            }

            public Blogs Blogs { get; set; }

            public Comment(object dto)
                : base(dto)
            {
                Blogs = new Blogs();
            }
        }

        Comments comments;

        Seed seed;

        dynamic blogId;

        dynamic commentId;

        dynamic comment;

        void associations_can_be_added_directly_to_gemini()
        {
            before = () =>
            {
                comments = new Comments();

                seed = new Seed();

                seed.PurgeDb();

                seed.CreateTable("Blogs", new dynamic[] 
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { Title = "nvarchar(255)" },
                    new { Body = "nvarchar(max)" }
                }).ExecuteNonQuery();

                seed.CreateTable("Comments", new dynamic[] 
                {
                    new { Id = "int", Identity = true, PrimaryKey = true },
                    new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                    new { Text = "nvarchar(1000)" }
                }).ExecuteNonQuery();

                blogId = new { Title = "Some Blog", Body = "Lorem Ipsum" }.InsertInto("Blogs");

                commentId = new { BlogId = blogId, Text = "Comment 1" }.InsertInto("Comments");
            };

            it["change tracking methods exist when changes is mixed in"] = () =>
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
