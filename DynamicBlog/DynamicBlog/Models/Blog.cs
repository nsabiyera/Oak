using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using System.Web.Mvc;

namespace DynamicBlog.Models
{
    public class Blog : DynamicModel
    {
        Comments comments;

        public Blog()
            : this(new { })
        {

        }

        public Blog(object valueType)
            : base(valueType)
        {
            comments = new Comments();

            Validates(new Presense { Property = "Title", Text = "Please specify a title for this blog post." });

            Validates(new Presense { Property = "Body" });
        }

        public void AddComment(string comment)
        {
            comments.Insert(new { BlogId = MixWith.Id, Text = comment });
        }

        public List<dynamic> Comments()
        {
            return comments.All("BlogId = @0", args: new[] { MixWith.Id }).ToList();
        }

        public string Summary
        {
            get
            {
                if (MixWith.Body == null) return "";

                if (MixWith.Body.Length > 50) return MixWith.Body.Substring(0, 50);

                return MixWith.Body;
            }
        }
    }
}