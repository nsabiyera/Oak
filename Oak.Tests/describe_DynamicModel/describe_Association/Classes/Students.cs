using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class StaticStudents : Students
    {
        public StaticStudents() : base("Students", "Id")
        {
            Projection = d => new StudentWithAutoProps(d).InitializeExtensions();
        }
    }

    public class Students : DynamicRepository
    {
        public Students() : this("Students", "Id")
        {
            
        }

        public Students(string tableName, string primaryKeyField) : base(tableName, primaryKeyField)
        {
            Projection = d => new Student(d);
        }
    }
}