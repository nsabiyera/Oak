using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using System.Web.Mvc;
using Massive;

namespace DynamicBlog.Models
{
    public class Blog : DynamicModel
    {
        Comments comments;

        public Blog(object valueType)
        {
            comments = new Comments();

            Validates(new Presense { Property = "Title", Text = "Please specify a title for this blog post." });

            Validates(new Presense { Property = "Body" });

            Associations(new HasMany(comments));

            Init(valueType);
        }

        public void AddComment(string comment)
        {
            comments.Insert(new { BlogId = Expando.Id, Text = comment });
        }

        public string Summary
        {
            get
            {
                if (Expando.Body == null) return "";

                if (Expando.Body.Length > 50) return Expando.Body.Substring(0, 50);

                return Expando.Body;
            }
        }
    }
}