using System.Collections.Generic;
using System;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {
        }

        public Person(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email") { ErrorMessage = "Email requires confirmation." };
        }
    }
    
    public class PersonWithDeferredErrorMessage : DynamicModel
    {
        public PersonWithDeferredErrorMessage()
            : this(new { })
        {
        }

        public PersonWithDeferredErrorMessage(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email") { ErrorMessage = new DynamicFunction(() => "Email requires confirmation.") };
        }
    }

    public class PersonWithAutoProps : DynamicModel
    {
        public string Email {get; set;}

        public string EmailConfirmation { get; set; }

        public PersonWithAutoProps()
            : this(new { })
        {
        }

        public PersonWithAutoProps(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Confirmation("Email") { ErrorMessage = "Email requires confirmation." };
        }
    }
}
