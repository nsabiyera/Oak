using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class select_has_many_and_belongs_to_relation : nspec
    {
        Seed seed;

        Students students;

        object studentId, courseId, courseId2;

        void before_each()
        {
            seed = new Seed();

            students = new Students();
        }

        void accessing_the_relation_through_an_enumerable()
        {
            before = () =>
            {
                seed.PurgeDb();

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

                courseId = new { Name = "History" }.InsertInto("Courses");

                courseId2 = new { Name = "Science" }.InsertInto("Courses");

                studentId = new { Name = "Amir" }.InsertInto("Students");

                new { studentId, courseId }.InsertInto("CoursesStudents");

                studentId = new { Name = "Doe" }.InsertInto("Students");

                new { studentId, courseId = courseId2 }.InsertInto("CoursesStudents");
            };
            
            it["performs a select many for all entries in the collection"] = () =>
            {
                var selectMany = (students.All() as dynamic).Courses();

                ((int)selectMany.Count()).should_be(2);
            };
        }
    }
}
