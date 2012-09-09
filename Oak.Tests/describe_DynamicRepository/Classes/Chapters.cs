using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace Oak.Tests.describe_DynamicRepository.Classes
{
    public class Chapters : DynamicRepository
    {
        public Chapters()
        {
            Projection = d => new Chapter(d);
        }
    }
}