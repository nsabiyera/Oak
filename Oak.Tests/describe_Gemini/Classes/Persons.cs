using System;
using Massive;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class Persons : DynamicRepository
    {
        public Persons()
            : base("Person")
        {
            Projection = d => new DynamicPerson(d);
        }
    }
}
