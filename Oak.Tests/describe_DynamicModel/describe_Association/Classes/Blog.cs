using System;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Blog : DynamicModel
    {
        Comments comments;

        Authors authors;

        public Blog(dynamic entity)
        {
            comments = new Comments();

            authors = new Authors();

            Associations(new HasMany(comments));

            Associations(new HasOne(authors));

            Init(entity);
        }
    }
}