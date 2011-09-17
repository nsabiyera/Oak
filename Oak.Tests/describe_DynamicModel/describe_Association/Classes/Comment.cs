using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Comment : DynamicModel
    {
        Blogs blogs;

        public Comment(dynamic dto)
        {
            blogs = new Blogs();

            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
            new BelongsTo(blogs);
        }
    }
}