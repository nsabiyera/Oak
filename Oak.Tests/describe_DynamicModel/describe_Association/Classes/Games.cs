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
    }
}
