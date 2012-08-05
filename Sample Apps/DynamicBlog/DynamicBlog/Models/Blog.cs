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
            comments.Insert(new { BlogId = _.Id, Text = comment });
        }

        public string Summary
        {
            get
            {
                if (_.Body == null) return "";

                if (_.Body.Length > 50) return _.Body.Substring(0, 50);

                return Prototype.Body;
            }
        }
    }
}