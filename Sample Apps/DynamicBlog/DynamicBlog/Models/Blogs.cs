using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

namespace DynamicBlog.Models
{
    public class Blogs : DynamicRepository
    {
        public Blogs()
        {
            Projection = (d) => new Blog(d);
        }

        public override IDictionary<string, object> GetAttributesToSave(object o)
        {
            return base.GetAttributesToSave(o).Exclude("Summary");
        }
    }
}