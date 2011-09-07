using System;

namespace Oak.Tests.SampleClasses
{
    public class Book : DynamicModel
    {
        public Book()
            : this(new { })
        {

        }

        public Book(dynamic o)
        {
            Validates(new Presense { Property = "Title" });
            Validates(new Presense { Property = "Body" });

            Init(o);
        }
    }
}