using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    class UnconventionalAuthors : DynamicRepository
    {
        public UnconventionalAuthors()
            : base("Authors")
        {
            Projection = d => new UnconventionalAuthor(d);
        }
    }
}
