using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.SampleClasses
{
    public class Blog : DynamicModel
    {
        Comments comments;

        public Blog(dynamic entity)
        {
            comments = new Comments();

            Associations(new HasMany(comments) { MethodToCreate = "Comments", ReferencedBy = "BlogId" });

            Init(entity);
        }
    }
}
