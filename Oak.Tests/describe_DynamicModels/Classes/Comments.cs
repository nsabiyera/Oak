using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Comments : DynamicRepository
    {
        public Comments()
        {
            Projection = d => new Comment(d);
        }
    }
}
