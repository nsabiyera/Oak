using System;

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
            Validates(new Presense("Title"));
            Validates(new Presense("Body"));

            Init(o);
        }
    }
}