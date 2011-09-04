using System;
using Oak.Models;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Book : DynamicModel
    {
        public Book()
        {
            Validates("Title", Presense);
            Validates("Body", Presense);
        }
    }
}