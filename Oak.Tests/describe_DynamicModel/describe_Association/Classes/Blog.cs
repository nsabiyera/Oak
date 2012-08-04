using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class BlogWithAutoProps : DynamicModel
    {
        Comments comments = new Comments();

        public BlogWithAutoProps()
            : this(new { })
        {

        }

        public BlogWithAutoProps(object entity)
            : base(entity)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
            new HasMany(comments) { ForeignKey = "BlogId" };

            yield return
            new HasMany(comments, "Roses") { ForeignKey = "BlogId" }; //a rose by any other name
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Body { get; set; }
    }

    public class Blog : DynamicModel
    {
        Comments comments = new Comments();

        public Blog()
            : this(new { })
        {

        }

        public Blog(object entity)
            : base(entity)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return 
            new HasMany(comments);

            yield return
            new HasMany(comments, "Roses"); //a rose by any other name
        }
    }
}