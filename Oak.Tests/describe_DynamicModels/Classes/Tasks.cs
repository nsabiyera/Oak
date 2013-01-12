using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Tasks : DynamicRepository
    {
        public Tasks()
        {
            Projection = d => new Task(d);
        }
    }
}
