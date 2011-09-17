using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_Validation.Classes
{
    public class Book : DynamicModel
    {
        public Book()
            : this(new { })
        {

        }

        public Book(dynamic o)
        {
            Init(o);
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Presense("Title");

            yield return new Presense("Body");
        }
    }
}