using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;

namespace DynamicBlog.Models
{
    public class Blog : Mix
    {
        public Blog(object valueType)
            : base(valueType)
        {
            
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