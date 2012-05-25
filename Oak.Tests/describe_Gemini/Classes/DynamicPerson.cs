using System;

namespace Oak.Tests.describe_Gemini.Classes
{
    public class DynamicPerson : DynamicModel
    {
        public DynamicPerson(object dto)
            : base(dto)
        {

        }

        dynamic SayHi()
        {
            return "Hi";
        }

        dynamic SayHello()
        {
            return "Hello";
        }
    }
}
