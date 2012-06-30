using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Student : DynamicModel
    {
        Courses courses = new Courses();

        Students students = new Students();

        public Student(object dto) : base(dto)
        {
        
        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(courses, students);
        }
    }
}
