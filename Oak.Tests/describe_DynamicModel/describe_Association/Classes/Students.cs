using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Students : DynamicRepository
    {
        public Students()
        {
            Projection = d => new Student(d);
        }
    }
}