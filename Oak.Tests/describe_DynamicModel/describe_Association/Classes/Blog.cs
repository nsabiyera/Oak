using System;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Blog : DynamicModel
    {
        Comments comments;

        public Blog(dynamic entity)
        {
            comments = new Comments();

            Associations(new HasMany(comments));

            Init(entity);
        }
    }
}