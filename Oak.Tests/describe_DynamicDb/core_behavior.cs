using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests
{
    class core_behavior : nspec
    {
        object blogId, blog2Id, authorId, math, science, history, jane, john;
        dynamic db;
        Seed seed;

        void before_each()
        {
            seed = new Seed();

            db = new DynamicDb();

            seed.PurgeDb();

            SeedBlogSchema();

            SeedSchoolSchema();
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

            new { authorId, Address = "user@example.com" }.InsertInto("Emails");

            new { blogId, Text = "Comment 1" }.InsertInto("Comments");

            new { blogId, Text = "Comment 2" }.InsertInto("Comments");
        }

        void specify_has_many()
        {
            var blog = db.Blogs().Single(blogId);

            var comments = blog.Comments();

            Expect(comments.Count()).to_be(2);
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
            var blog = db.Blogs().Single(blogId);

            Expect(blog.Author().Name).to_be("Amir");

            Expect(blog.Author().Email().Address).to_be("user@example.com");
        }

        void specify_many_to_many()
        {
            var student = db.Students().Single(john);

            Expect(student.Courses().Count()).to_be(2);

            Expect(student.Courses().Select("Name")).to_contain("Math");

            Expect(student.Courses().Select("Name")).to_contain("Science");

            student = db.Students().Single(jane);

            Expect(student.Courses().Count()).to_be(2);

            Expect(student.Courses().Select("Name")).to_contain("Math");

            Expect(student.Courses().Select("Name")).to_contain("History");

            var course = db.Courses().Single(math);

            Expect(course.Students().Count()).to_be(2);

            Expect(course.Students().Select("Name")).to_contain("Jane");

            Expect(course.Students().Select("Name")).to_contain("John");

            course = db.Courses().Single(science);

            Expect(course.Students().Count()).to_be(1);

            Expect(course.Students().Select("Name")).to_contain("John");

            course = db.Courses().Single(history);

            Expect(course.Students().Count()).to_be(1);

            Expect(course.Students().Select("Name")).to_contain("Jane");
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
