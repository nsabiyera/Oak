using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

namespace DynamicBlog.Models
{
    public class Blogs : DynamicModel
    {
        public Blogs()
        {
            Projection = (d) => new Blog(d);
        }
    }
}