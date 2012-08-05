using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Games : DynamicRepository 
    {
        public Games()
        {
            Projection = d => new Game(d);
        }

        public override object Insert(dynamic o)
        {
            return base.Insert(o.Exclude("Id") as object);
        }
    }
}
