using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.SampleClasses
{
    public class Blogs : DynamicRepository
    {
        public Blogs()
        {
            Projection = d => new Blog(d);
        }
    }
}