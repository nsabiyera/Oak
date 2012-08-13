using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Screencasts : DynamicRepository
    {
        public Screencasts()
        {
            Projection = d => new Screencast(d);
        }
    }
}
