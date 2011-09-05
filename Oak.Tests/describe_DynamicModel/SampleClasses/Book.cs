using System;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Book : DynamicModel
    {
        public Book()
            : this(new { })
        {

        }

        public Book(dynamic entity)
            : base(entity as object)
        {
            Validates(new Presense { Property = "Title" });
            Validates(new Presense { Property = "Body" });
        }
    }
}