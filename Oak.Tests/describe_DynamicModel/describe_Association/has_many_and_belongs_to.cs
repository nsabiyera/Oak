using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes.Unconventional;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    [Tag("wip")]
    class has_many_and_belongs_to_static_type : has_many_and_belongs_to
    {
        void before_each()
        {
            students = new StaticStudents();
        }

        void describe_has_many_and_belongs_to()
        {
            before = () =>
            {
                seed.PurgeDb();

                BuildCommonTables();

                seed.CreateTable("CoursesStudents", new dynamic[] 
                { 
                    seed.Id(),
                    new { CourseId = "int" },
                    new { StudentId = "int" }
                }).ExecuteNonQuery();

                studentId = new { Name = "Amir" }.InsertInto("Students");

                courseId = new { Name = "History" }.InsertInto("Courses");

                new { studentId, courseId }.InsertInto("CoursesStudents");
            };

            it["cross reference table is determined by convention (tables are alphabetically combined)"] = () =>
                (FirstCourseFor(studentId).Name as string).should_be("History");

            it["has a reference back to the original table"] = () =>
                (FirstCourseFor(studentId).StudentWithAutoProps.Name as string).should_be("Amir");

            it["new association can be created from reference"] = () =>
            {
                var newCourse = CoursesFor(studentId).New(name: "Science");

                (newCourse is StaticCourse).should_be_true();

                (newCourse.Name as string).should_be("Science");
            };

            context["cacheing"] = () =>
            {
                before = () =>
                {
                    student = students.Single(studentId);

                    student.Courses();
                };

                it["collection is cached until discarded"] = () =>
                {
                    var newCourse = new { Name = "Science" }.InsertInto("Courses");

                    new { studentId, courseId = newCourse }.InsertInto("CoursesStudents");

                    int cachedCount = student.Courses().Count();

                    cachedCount.should_be(1);

                    int newCount = student.Courses(discardCache: true).Count();

                    newCount.should_be(2);
                };
            };

            context["retrieving ids"] = () =>
            {
                it["contains just ids"] = () =>
                {
                    student = students.Single(studentId);

                    var ids = student.CourseIds() as IEnumerable<dynamic>;

                    (ids.First() as object).should_be(courseId as object);
                };
            };
        }
    }

    class has_many_and_belongs_to_conventional_schema : has_many_and_belongs_to
    {
        void describe_has_many_and_belongs_to()
        {
            before = () =>
            {
                seed.PurgeDb();

                BuildCommonTables();

                seed.CreateTable("CoursesStudents", new dynamic[] 
                { 
                    seed.Id(),
                    new { CourseId = "int" },
                    new { StudentId = "int" }
                }).ExecuteNonQuery();

                studentId = new { Name = "Amir" }.InsertInto("Students");

                courseId = new { Name = "History" }.InsertInto("Courses");

                new { studentId, courseId }.InsertInto("CoursesStudents");
            };

            it["cross reference table is determined by convention (tables are alphabetically combined)"] = () =>
                (FirstCourseFor(studentId).Name as string).should_be("History");

            it["has a reference back to the original table"] = () =>
                (FirstCourseFor(studentId).Student.Name as string).should_be("Amir");

            it["new association can be created from reference"] = () =>
            {
                var newCourse = CoursesFor(studentId).New(name: "Science");

                (newCourse is Course).should_be_true();

                (newCourse.Name as string).should_be("Science");
            };

            context["cacheing"] = () =>
            {
                before = () =>
                {
                    student = students.Single(studentId);

                    student.Courses();
                };

                it["collection is cached until discarded"] = () =>
                {
                    var newCourse = new { Name = "Science" }.InsertInto("Courses");

                    new { studentId, courseId = newCourse }.InsertInto("CoursesStudents");

                    int cachedCount = student.Courses().Count();

                    cachedCount.should_be(1);

                    int newCount = student.Courses(discardCache: true).Count();

                    newCount.should_be(2);
                };
            };

            context["retrieving ids"] = () =>
            {
                it["contains just ids"] = () =>
                {
                    student = students.Single(studentId);

                    var ids = student.CourseIds() as IEnumerable<dynamic>;

                    (ids.First() as object).should_be(courseId as object);
                };
            };
        }
    }

    class has_many_and_belongs_to_unconventional_schema : has_many_and_belongs_to
    {
        void describe_overrides()
        {
            context["xref table can be overridden"] = () =>
            {
                before = () =>
                {
                    seed.PurgeDb();

                    BuildCommonTables();

                    seed.CreateTable("StudentsCourses", new dynamic[] 
                    { 
                        seed.Id(),
                        new { CourseId = "int" },
                        new { StudentId = "int" }
                    }).ExecuteNonQuery();

                    studentId = new { Name = "Amir" }.InsertInto("Students");

                    courseId = new { Name = "History" }.InsertInto("Courses");

                    new { studentId, courseId }.InsertInto("StudentsCourses");
                };

                it["works"] = () =>
                {
                    UnconventionalStudents uStudents = new UnconventionalStudents();

                    var student = uStudents.Single(studentId);

                    (student.Courses().First().Name as string).should_be("History");
                };
            };
        }
    }

    abstract class has_many_and_belongs_to : nspec
    {
        public Seed seed;

        public dynamic student;

        public object studentId, courseId;

        public Students students;

        void before_each()
        {
            seed = new Seed();

            students = new Students();
        }

        public void BuildCommonTables()
        {
            seed.CreateTable("Students", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

            seed.CreateTable("Courses", new dynamic[] 
                { 
                    seed.Id(),
                    new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();
        }

        public dynamic CoursesFor(object studentId)
        {
            return students.Single(studentId).Courses();
        }

        public dynamic FirstCourseFor(object studentId)
        {
            return CoursesFor(studentId).First();
        }
    }
}
