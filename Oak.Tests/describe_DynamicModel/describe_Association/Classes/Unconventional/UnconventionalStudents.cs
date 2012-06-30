using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes.Unconventional
{
    class UnconventionalStudents : DynamicRepository
    {
        public UnconventionalStudents()
            : base("Students")
        {
            Projection = d => new UnconventionalStudent(d);
        }
    }

    class UnconventionalCourses : DynamicRepository
    {
        public UnconventionalCourses()
            : base("Courses")
        {
            
        }
    }

    class UnconventionalStudent : DynamicModel
    {
        UnconventionalStudents students = new UnconventionalStudents();

        UnconventionalCourses courses = new UnconventionalCourses();

        public UnconventionalStudent(object dto)
            : base(dto)
        {
            
        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(courses, students)
            {
                Named = "Courses",
                CrossRefenceTable = "StudentsCourses",
                FromColumn = "StudentId",
                ForeignKey = "CourseId"
            };
        }
    }
}
