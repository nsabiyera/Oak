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

        public override object Insert(object o)
        {
            return base.Insert(ExcludeProps(o));
        }

        public override int Update(object o, object key)
        {
            return base.Update(ExcludeProps(o), key);
        }

        public override void Save(params object[] things)
        {
            base.Save(things.Select(ExcludeProps));
        }

        public object ExcludeProps(dynamic o)
        {
            return o.Exclude("Summary");
        }
    }
}