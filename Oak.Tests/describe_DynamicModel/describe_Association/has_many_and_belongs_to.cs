using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes.Unconventional;

namespace Oak.Tests.describe_DynamicModel.describe_Association
{
    class has_many_and_belongs_to_eager_loading : has_many_and_belongs_to
    {
        public object student1Id, student2Id, course1Id, course2Id;

        dynamic studentsResult;

        dynamic coursesResult;

        void before_each()
        {
            seed.PurgeDb();

            BuildCommonTables();

            student1Id = new { Name = "Jane" }.InsertInto("Students");

            student2Id = new { Name = "John" }.InsertInto("Students");

            course1Id = new { Name = "History" }.InsertInto("Courses");

            course2Id = new { Name = "Science" }.InsertInto("Courses");

            new { studentId = student1Id, courseId = course1Id }.InsertInto("CoursesStudents");

            new { studentId = student2Id, courseId = course2Id }.InsertInto("CoursesStudents");
        }

        void retrieval_of_relationship_is_performed_at_the_collection_level()
        {
            act = () =>
            {
                studentsResult = students.All();

                coursesResult = studentsResult.Courses();
            };

            it["has many and belongs to relation has reference back to parent"] = () =>
            {
                (coursesResult.First().Student.Id as object).should_be(student1Id);

                (coursesResult.Last().Student.Id as object).should_be(student2Id);
            };

            it["the associations are cached"] = () =>
            {
                var firstStudent = studentsResult.First();

                ((int)firstStudent.Courses().Count()).should_be(1);

                new { studentId = firstStudent.Id, courseId = course2Id }.InsertInto("CoursesStudents");

                ((int)firstStudent.Courses().Count()).should_be(1);
            };
        }
    }

    class saving_has_many_belongs_to : has_many_and_belongs_to
    {
        public object student1Id, student2Id, course1Id, course2Id;

        dynamic studentsResult;

        dynamic coursesResult;

        void before_each()
        {
            seed.PurgeDb();

            BuildCommonTables();

            student1Id = new { Name = "Jane" }.InsertInto("Students");

            student2Id = new { Name = "John" }.InsertInto("Students");

            course1Id = new { Name = "History" }.InsertInto("Courses");

            course2Id = new { Name = "Science" }.InsertInto("Courses");
        }

        void it_works()
        {
            students.All().Count().should_be(2);

            var student = students.Single(student1Id);

            var repository = student.Courses().Repository;

            student.AssociateCourse(course1Id);

            student = students.Single(student1Id);

            (student.Courses().Count() as object).should_be(1);
        }
    }

    class has_many_and_belongs_to_static_type : has_many_and_belongs_to
    {
        void before_each()
        {
            students = new StaticStudents();

            seed.PurgeDb();

            BuildCommonTables();

            studentId = new { Name = "Amir" }.InsertInto("Students");

            courseId = new { Name = "History" }.InsertInto("Courses");

            new { studentId, courseId }.InsertInto("CoursesStudents");
        }

        void specify_cross_reference_is_determined_by_convention()
        {
            (FirstCourseFor(studentId).Name as string).should_be("History");
        }

        void it_has_a_reference_back_to_the_original_record()
        {
            (FirstCourseFor(studentId).StudentWithAutoProps.Name as string).should_be("Amir");
        }

        void specify_new_association_can_be_created_from_reference()
        {
            var newCourse = CoursesFor(studentId).New(name: "Science");

            (newCourse is StaticCourse).should_be_true();

            (newCourse.Name as string).should_be("Science");
        }

        void specify_retrieving_ids()
        {
            student = students.Single(studentId);

            var ids = student.CourseIds() as IEnumerable<dynamic>;

            (ids.First() as object).should_be(courseId as object);
        }

        void describe_cacheing()
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
        }
    }

    class has_many_and_belongs_to_conventional_schema : has_many_and_belongs_to
    {
        void before_each()
        {
            seed.PurgeDb();

            BuildCommonTables();

            studentId = new { Name = "Amir" }.InsertInto("Students");

            courseId = new { Name = "History" }.InsertInto("Courses");

            new { studentId, courseId }.InsertInto("CoursesStudents");
        }

        void specify_cross_reference_table_is_determined_by_conventions()
        {
            (FirstCourseFor(studentId).Name as string).should_be("History");
        }

        void it_has_a_reference_back_to_the_original_table()
        {
            (FirstCourseFor(studentId).Student.Name as string).should_be("Amir");
        }

        void specify_a_new_reference_can_be_created_from_relationship()
        {
            var newCourse = CoursesFor(studentId).New(name: "Science");

            (newCourse is Course).should_be_true();

            (newCourse.Name as string).should_be("Science");
        }

        void describe_cacheing()
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
        }

        void specify_retrieving_ids()
        {
            student = students.Single(studentId);

            var ids = student.CourseIds() as IEnumerable<dynamic>;

            (ids.First() as object).should_be(courseId as object);
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

            seed.CreateTable("CoursesStudents", new dynamic[] 
            { 
                seed.Id(),
                new { CourseId = "int" },
                new { StudentId = "int" }
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
