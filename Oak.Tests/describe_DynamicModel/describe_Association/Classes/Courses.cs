using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Courses : DynamicRepository
    {
        public Courses()
        {
            Projection = d => new Course(d);
        }
    }

    public class Course : DynamicModel
    {
        public Course(object dto) : base(dto)
        {
            
        }
    }
}
