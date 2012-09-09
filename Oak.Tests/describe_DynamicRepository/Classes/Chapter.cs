using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicRepository.Classes
{
    public class Chapter : DynamicModel
    {
        Books books = new Books();

        public Chapter(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(books);
        }
    }
}
