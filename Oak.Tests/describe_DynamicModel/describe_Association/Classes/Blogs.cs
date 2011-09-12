using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Blogs : DynamicRepository
    {
        public Blogs()
        {
            Projection = d => new Blog(d);
        }
    }
}
