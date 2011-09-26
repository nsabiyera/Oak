using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class UnconventionalComment2 : DynamicModel
    {
        Blogs blogs;

        public UnconventionalComment2(dynamic dto)
        {
            blogs = new Blogs();

            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
                new BelongsTo(blogs) { PrimaryKey = "BlogId" };
        }
    }
}
