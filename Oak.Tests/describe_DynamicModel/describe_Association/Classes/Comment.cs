using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class CommentWithAutoProps : DynamicModel
    {
        Blogs blogs = new Blogs();

        public CommentWithAutoProps(object dto)
            : base(dto)
        {
            
        }
        public CommentWithAutoProps()
        {
            
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
            new BelongsTo(blogs);
        }

        public int Id { get; set; }
        public int BlogId { get; set; }
        public string Text { get; set; }
    }

    public class Comment : DynamicModel
    {
        Blogs blogs = new Blogs();

        public Comment(object dto)
            : base(dto)
        {
            
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
            new BelongsTo(blogs) { PropertyContainingIdValue = "BlogId" };
        }
    }
}