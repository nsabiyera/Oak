using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicRepository.Classes
{
    public class Book : DynamicModel
    {
        Chapters chapters = new Chapters();

        public Book(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(chapters);
        }
    }
}
