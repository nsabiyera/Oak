using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicDb
{
    [Tag("wip")]
    class helpful_errors : nspec
    {
        dynamic db;
        Seed seed;

        void before_each()
        {
            db = new DynamicDb();

            AssociationByConventions.ColumnCache = new Dictionary<string, List<string>>();
            AssociationByConventions.TableCache = new Dictionary<string, bool>();

            seed = new Seed();

            seed.PurgeDb();
        }

        void it_tells_you_if_table_doesnt_exist()
        {
            try
            {
                db.Foobars();
                Exception();
            }
            catch (InvalidOperationException ex)
            {
                ex.Message.should_contain("Table [Foobars] does not exist.");
            }
        }

        object blogId;
        void has_many_help()
        {
            context["has many table doesn't exist"] = () =>
            {
                before = () =>
                {
                    seed.CreateTable("Blogs",
                        seed.Id(),
                        new { Title = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    blogId = new { Title = "a blog" }.InsertInto("Blogs");
                };

                it["provides a helpful error message"] = () =>
                {
                    try
                    {
                        db.Blogs().Single(blogId).Comments();
                        Exception();
                    }
                    catch (AssociationConventionException ex)
                    {
                        ex.Message.should_contain("No HasMany or HasManyAndBelongsTo relationships found:");
                        ex.Message.should_contain("Table [Comments] with column [BlogId] doesn't exist (HasMany).");
                        ex.Message.should_contain("Table [BlogsComments] with schema [Id, BlogId, CommentId] doesn't exist (HasManyAndBelongsTo).");
                    }
                };
            };

            context["has many table exists, but columns aren't right"] = () =>
            {
                before = () =>
                {
                    seed.CreateTable("Blogs",
                        seed.Id(),
                        new { Title = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    seed.CreateTable("Comments",
                        seed.Id(),
                        new { fk_BlogId = "int" },
                        new { Text = "nvarchar(max)" }
                    ).ExecuteNonQuery();

                    var blogId = new { Title = "a blog" }.InsertInto("Blogs");
                };

                it["provides a helpful error message"] = () =>
                {
                    try
                    {
                        db.Blogs().Single(blogId).Comments();
                        Exception();
                    }
                    catch (AssociationConventionException ex)
                    {
                        ex.Message.should_contain("Table [Comments] with column [BlogId] doesn't exist (HasMany).");
                    }
                };
            };
        }

        object math, jane;
        void has_many_and_belongs_to_help()
        {
            context["has many belongs to table doesn't exist"] = () =>
            {
                before = () =>
                {
                    seed.CreateTable("Courses",
                        seed.Id(),
                        new { Name = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    seed.CreateTable("Students",
                        seed.Id(),
                        new { Name = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    math = new { Name = "Math" }.InsertInto("Courses");

                    jane = new { Name = "Jane" }.InsertInto("Students");
                };

                it["provides a helpful error message"] = () =>
                {
                    try
                    {
                        db.Courses().Single(math).Students();
                        Exception();
                    }
                    catch (AssociationConventionException ex)
                    {
                        ex.Message.should_contain("No HasMany or HasManyAndBelongsTo relationships found:");
                        ex.Message.should_contain("Table [Students] with column [CourseId] doesn't exist (HasMany).");
                        ex.Message.should_contain("Table [CoursesStudents] with schema [Id, CourseId, StudentId] doesn't exist (HasManyAndBelongsTo).");
                    }
                };
            };

            context["has many belongs to columns aren't right"] = () =>
            {
                before = () =>
                {
                    seed.CreateTable("Courses",
                        seed.Id(),
                        new { Name = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    seed.CreateTable("Students",
                        seed.Id(),
                        new { Name = "nvarchar(255)" }
                    ).ExecuteNonQuery();

                    seed.CreateTable("CoursesStudents",
                        seed.Id(),
                        new { CourseId = "int" },
                        new { fk_StudentId = "int" }
                    ).ExecuteNonQuery();

                    math = new { Name = "Math" }.InsertInto("Courses");

                    jane = new { Name = "Jane" }.InsertInto("Students");
                };

                it["provides a helpful error message"] = () =>
                {
                    try
                    {
                        db.Courses().Single(math).Students();
                        Exception();
                    }
                    catch (AssociationConventionException ex)
                    {
                        ex.Message.should_contain("No HasMany or HasManyAndBelongsTo relationships found:");
                        ex.Message.should_contain("Table [Students] with column [CourseId] doesn't exist (HasMany).");
                        ex.Message.should_contain("Table [CoursesStudents] with schema [Id, CourseId, StudentId] doesn't exist (HasManyAndBelongsTo).");
                    }
                };
            };
        }

        object commentId;
        void belongs_to_help()
        {
            context["belongs to table doesn't exist"] = () =>
            {
                before = () =>
                {
                    seed.CreateTable("Comments",
                        seed.Id(),
                        new { BlogId = "int" },
                        new { Text = "nvarchar(max)" }
                    ).ExecuteNonQuery();

                    commentId = new { BlogId = 10, Text = "a comment" }.InsertInto("Comments");
                };

                it["gives a helpful error message"] = () =>
                {
                    try
                    {
                        db.Comments().Single(commentId).Blog();
                        Exception();
                    }
                    catch (AssociationConventionException ex)
                    {
                        ex.Message.should_contain("No BelongsTo or HasOne relationships found:");
                        ex.Message.should_contain("Table [Blogs] with column [CommentId] doesn't exist (HasOne).");
                        ex.Message.should_contain("Table [Comments] with column [BlogId] doesn't exist (BelongsTo).");
                    }
                };
            };
        }

        void Exception()
        {
            throw new SystemException("No exception was thrown when one was expected");
        }
    }
}
