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
        Comments comments = new Comments();

        public Blog()
            : this(new { })
        {
        }

        public Blog(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presence("Title") { ErrorMessage = "Please specify a title for this blog post." };

            yield return new Presence("Body");
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(comments);
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