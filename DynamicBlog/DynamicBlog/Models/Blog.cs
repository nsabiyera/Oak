using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;

namespace DynamicBlog.Models
{
    public class Blog : Mix
    {
        Comments comments;

        public Blog(object valueType)
            : base(valueType)
        {
            comments = new Comments();
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(MixWith.Title);
        }

        public string Validate()
        {
            if(!IsValid())
            {
                return "Title Required.";
            }

            return "";
        }

        public void AddComment(string comment)
        {
            comments.Insert(new { BlogId = MixWith.Id, Text = comment });
        }

        public List<dynamic> Comments()
        {
            return (comments.All("BlogId = @0", "", 0, "*", MixWith.Id) as IEnumerable<dynamic>).ToList();
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