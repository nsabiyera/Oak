using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Library : DynamicRepository 
    {
        public override object Insert(dynamic o)
        {
            return base.Insert(o.Exclude("Id") as object);
        }
    }
}
