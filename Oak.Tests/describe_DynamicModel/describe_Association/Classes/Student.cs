using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class StudentWithAutoProps : DynamicModel
    {
        StaticCourses courses = new StaticCourses();

        StaticStudents students = new StaticStudents();

        public StudentWithAutoProps(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(courses, students)
            {
                Named = "Courses",
                ForeignKey = "CourseId",
                FromColumn = "StudentId"
            };
        }

        public string Name { get; set; }
    }

    public class Student : DynamicModel
    {
        Courses courses = new Courses();

        Students students = new Students();

        public Student(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(courses, students);
        }

        void AssociateCourse(dynamic courseId)
        {
            _.Courses().Repository.Insert(new { studentId = _.Id, courseId });
        }
    }
}
