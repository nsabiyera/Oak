using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class UnconventionalAuthor : DynamicModel
    {
        Authors authors;

        public UnconventionalAuthor(dynamic dto)
        {
            authors = new Authors();

            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return
                new HasOne(authors) { ForeignKey = "fkAuthorId" };
        }
    }
}
