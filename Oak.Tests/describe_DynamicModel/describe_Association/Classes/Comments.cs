using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Comments : DynamicRepository
    {
        public Comments()
        {
            Projection = d => new Comment(d);
        }
    }
}
