using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using System.Configuration;

namespace Oak.Tests
{
    [Tag("Association")]
    class core_behavior_dynamic_db : nspec
    {
        object blogId, blog2Id, authorId, author2Id, math, science, history, jane, john;
        dynamic db;
        Seed seed;

        void before_each()
        {
            seed = new Seed();

            db = new DynamicDb();

            seed.PurgeDb();

            SeedBlogSchema();

            SeedSchoolSchema();

            SeedPeepsSchema();
        }

        void specify_db_can_be_specified()
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AnotherDb"].ConnectionString;

            seed = new Seed(new ConnectionProfile
            {
                ConnectionString = connectionString
            });

            seed.PurgeDb();

            seed.CreateTable("Authors",
                seed.Id(),
                new { Name = "nvarchar(255)" }
            ).ExecuteNonQuery(seed.ConnectionProfile);

            seed.CreateTable("Emails",
                seed.Id(),
                new { AuthorId = "int" },
                new { Address = "nvarchar(255)" }
            ).ExecuteNonQuery(seed.ConnectionProfile);

            db = new DynamicDb(connectionString);

            var authorId = db.Authors().Insert(new { Name = "hello" });

            db.Emails().Insert(new { authorId, Address = "user@example.com" });

            (db.Authors().All().First().Email().Address as string).should_be("user@example.com");
        }

        void specify_has_many()
        {
            var blog = db.Blogs().Single(blogId);

            var comments = blog.Comments();

            Expect(comments.Count()).to_be(2);

            Expect(comments.Select("Text"))
                .to_contain("Comment 1")
                .to_contain("Comment 2");
        }

        void specify_belongs_to()
        {
            var comment = db.Comments().All().First();

            Expect(comment.Blog().Title).to_be("a blog");
        }

        void specify_belongs_to_missing_relation()
        {
            var blog = db.Blogs().Single(blog2Id);

            Expect(blog.Author()).to_be(null);
        }

        void specify_has_one()
        {
            var author = db.Authors().Single(authorId);

            Expect(author.Email().Address).to_be("user@example.com");
        }

        void specify_has_one_missing()
        {
            var author = db.Authors().Single(author2Id);

            Expect(author.Email()).to_be(null);
        }

        object peepId, peep2Id, locationId;
        void specify_has_one_through()
        {
            var peep = db.Peeps().Single(peepId);
            Expect(peep.Location().Street).to_be("Main");

            peep = db.Peeps().Single(peep2Id);
            Expect(peep.Location().Street).to_be("Main");
        }

        void specify_many_to_many()
        {
            var student = db.Students().Single(john);

            Expect(student.Courses().Count()).to_be(2);

            Expect(student.Courses().Select("Name"))
                .to_contain("Math")
                .to_contain("Science");

            student = db.Students().Single(jane);

            Expect(student.Courses().Count()).to_be(2);

            Expect(student.Courses().Select("Name"))
                .to_contain("Math")
                .to_contain("History");

            var course = db.Courses().Single(math);

            Expect(course.Students().Count()).to_be(2);

            Expect(course.Students().Select("Name"))
                .to_contain("Jane")
                .to_contain("John");

            course = db.Courses().Single(science);

            Expect(course.Students().Count()).to_be(1);

            Expect(course.Students().Select("Name")).to_contain("John");

            course = db.Courses().Single(history);

            Expect(course.Students().Count()).to_be(1);

            Expect(course.Students().Select("Name")).to_contain("Jane");
        }

        void specify_eager_load_many()
        {
            List<string> queriesExecuted = new List<string>();

            DynamicRepository.WriteDevLog = true;
            DynamicRepository.LogSql = (o, sql, args) => queriesExecuted.Add(sql);

            var blogs = db.Blogs().All().Include("Comments");
            var comments = blogs.First().Comments();

            Expect(comments.Count()).to_be(2);

            queriesExecuted.Count(s => s.Trim().Contains("from Comments")).should_be(1);
        }

        void specify_eager_load_single()
        {
            var peeps = db.Peeps().All().Include("Location");

            Expect(peeps.First().Location().Street).to_be("Main");
        }

        void ancillary_methods_for_has_many()
        {
            context["[Association]Ids method provides correct values"] = () =>
            {
                it["works"] = () => 
                {
                    var blog = db.Blogs().Single(blogId);

                    var commentsIds = blog.CommentIds();

                    Expect(commentsIds.Count).to_be(2);
                };
            };

            context["New[Association] method provides correct values"] = () =>
            {
                it["works for no parameters"] = () =>
                {
                    var blog = db.Blogs().Single(blogId);
                    var comment = blog.NewComment();
                    Expect(comment.RespondsTo("text")).to_be(false);
                };

                it["works for named parameters"] = () =>
                {
                    var blog = db.Blogs().Single(blogId);
                    var comment = blog.NewComment(text: "text");
                    Expect(comment.Text).to_be("text");
                };

                it["works for anonymous type"] = () => 
                {
                    var blog = db.Blogs().Single(blogId);
                    var comment = blog.NewComment(new { Text = "text" });
                    Expect(comment.BlogId).to_be(blogId);
                    Expect(comment.Text).to_be("text");
                };
            };
        }

        void SeedPeepsSchema()
        {
            seed.CreateTable("Peeps",
                seed.Id(),
                new { Name = "nvarchar(255)" }
            ).ExecuteNonQuery();

            seed.CreateTable("LocationsPeeps",
                seed.Id(),
                new { LocationId = "int" },
                new { PeepId = "int" }
            ).ExecuteNonQuery();

            seed.CreateTable("Locations",
                seed.Id(),
                new { Street = "nvarchar(255)" }
            ).ExecuteNonQuery();

            peepId = new { Name = "Jane" }.InsertInto("Peeps");
            peep2Id = new { Name = "John" }.InsertInto("Peeps");
            locationId = new { Street = "Main" }.InsertInto("Locations");

            new { PeepId = peepId, LocationId = locationId }.InsertInto("LocationsPeeps");
            new { PeepId = peep2Id, LocationId = locationId }.InsertInto("LocationsPeeps");
        }

        void SeedSchoolSchema()
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
                new { StudentId = "int" }
            ).ExecuteNonQuery();

            math = new { Name = "Math" }.InsertInto("Courses");
            science = new { Name = "Science" }.InsertInto("Courses");
            history = new { Name = "History" }.InsertInto("Courses");

            jane = new { Name = "Jane" }.InsertInto("Students");
            john = new { Name = "John" }.InsertInto("Students");

            new { studentId = john, courseId = math }.InsertInto("CoursesStudents");
            new { studentId = john, courseId = science }.InsertInto("CoursesStudents");

            new { studentId = jane, courseId = math }.InsertInto("CoursesStudents");
            new { studentId = jane, courseId = history }.InsertInto("CoursesStudents");
        }

        void SeedBlogSchema()
        {
            seed.CreateTable("Authors",
                seed.Id(),
                new { Name = "nvarchar(255)" }
            ).ExecuteNonQuery();

            seed.CreateTable("Emails",
                seed.Id(),
                new { AuthorId = "int" },
                new { Address = "nvarchar(255)" }
            ).ExecuteNonQuery();

            seed.CreateTable("Blogs",
                seed.Id(),
                new { Title = "nvarchar(255)" },
                new { AuthorId = "int" }
            ).ExecuteNonQuery();

            seed.CreateTable("Comments",
                seed.Id(),
                new { BlogId = "int", ForeignKey = "Blogs(Id)" },
                new { Text = "nvarchar(max)" }
           ).ExecuteNonQuery();

            authorId = new { Name = "Amir" }.InsertInto("Authors");

            blogId = new { Title = "a blog", authorId }.InsertInto("Blogs");

            blog2Id = new { Title = "a blog" }.InsertInto("Blogs");

            authorId = new { authorId, Address = "user@example.com" }.InsertInto("Emails");

            author2Id = new { Name = "Jane" }.InsertInto("Authors");

            new { blogId, Text = "Comment 1" }.InsertInto("Comments");

            new { blogId, Text = "Comment 2" }.InsertInto("Comments");
        }

        Expect Expect(object o)
        {
            return new Expect(o);
        }
    }

    public class Expect
    {
        object o;

        public Expect(object o)
        {
            this.o = o;
        }

        public Expect to_be(object actual)
        {
            o.should_be(actual);

            return this;
        }

        public Expect to_contain(object actual)
        {
            (o as IEnumerable<dynamic>).should_contain(actual);

            return this;
        }
    }
}
