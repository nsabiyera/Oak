using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    class UnconventionalComments : DynamicRepository
    {
        public UnconventionalComments()
            : base("Comments")
        {
            Projection = d => new UnconventionalComment(d);
        }
    }
}
