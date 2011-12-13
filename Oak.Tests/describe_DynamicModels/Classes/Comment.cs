using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Comment : DynamicModel
    {
        Posts posts = new Posts();

        public Comment(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(posts);
        }
    }
}
