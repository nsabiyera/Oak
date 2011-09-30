using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class UnconventionalComments2 : DynamicRepository
    {
        public UnconventionalComments2()
            : base("Comments")
        {
            Projection = d => new UnconventionalComment2(d);
        }
    }
}
