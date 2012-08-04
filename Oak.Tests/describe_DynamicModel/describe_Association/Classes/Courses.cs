using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;
using Oak;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class StaticCourses : Courses
    {
        public StaticCourses()
            : base("Courses", "Id")
        {
            Projection = d => new StaticCourse(d).InitializeExtensions();
        }
    }

    public class Courses : DynamicRepository
    {
        public Courses(string tableName = "", string primaryKeyField = "")
            : base(tableName, primaryKeyField)
        {
            Projection = d => new Course(d);
        }
    }

    public class StaticCourse : DynamicModel
    {
        public StaticCourse(object dto) : base(dto)
        {
            
        }

        public string Name { get; set; }
    }

    public class Course : DynamicModel
    {
        public Course(object dto) : base(dto)
        {
            
        }
    }
}
