using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using NUnit.Framework;
using Oak;

namespace DynamicBlog.Tests
{
    public class MyDynamic : DynamicObject
    {
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = binder.Name.ToUpper();
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            return base.TrySetMember(binder, value);
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            return base.TryInvokeMember(binder, args, out result);
        }
    }

    class describe_dynamic : nspec
    {
        void specify_ExpandoObject()
        {
            dynamic person = new ExpandoObject();

            person.FirstName = "Jane"; //try set member

            Assert.AreEqual("Jane", person.FirstName); //try get member

            person.SayHello = new Func<string>(() => "hello"); //try set member

            Assert.AreEqual("hello", person.SayHello()); //try invoke member
        }

        void specify_DynamicObject()
        {
            dynamic ourDynamic = new MyDynamic();

            Assert.AreEqual("FIRSTNAME", ourDynamic.FirstName);
        }

        void specify_some_usage()
        {
            Assert.AreEqual(true, IsValid(new { Name = "Amir" }));

            Assert.AreEqual(true, IsValid(new AssertionException("some exception").GetType()));
        }

        void specify_taking_it_to_the_next_level()
        {
            dynamic person = new Person();

            person.FirstName = "Amir";

            Assert.AreEqual("Amir", person.FirstName);

            Assert.AreEqual(true, person.RespondsTo("FirstName"));

            person.SetMember("FirstName", "Jane");

            Assert.AreEqual("Jane", person.FirstName);

            Assert.AreEqual("Jane", person.GetMember("FirstName"));

            person.DeleteMember("FirstName");

            Assert.AreEqual(false, person.RespondsTo("FirstName"));

            person = new Person(new { FirstName = "Amir" });

            Assert.AreEqual(false, person.HasChanged());

            person.FirstName = "Jane";

            Assert.AreEqual(true, person.HasChanged());

            Assert.AreEqual("Amir", person.Original("FirstName"));

            var change = person.Changes("FirstName");

            Assert.AreEqual("Amir", change.Original);

            Assert.AreEqual("Jane", change.New);

            Assert.AreEqual(true, person.IsValid());  //this should throw exception if there is no validation defined

            person.FirstName = "";

            Assert.AreEqual(false, person.IsValid());

            Assert.AreEqual("First name is required.", person.FirstError());
        }

        void specify_dynamic_validation()
        {
            dynamic person = new Person();

            person.FirstName = "Amir";

            person.Email = "ar@amirrajan.net";

            person.EmailConfirmation = "";

            Assert.AreEqual(false, person.IsValid());

            Assert.AreEqual("Emails must match.", person.FirstError());

            Assert.AreEqual("AMIR", person.UpperCaseFirstName());
        }

        public bool IsValid(dynamic entity)
        {
            if (entity.Name.StartsWith("A")) return true;

            else return false;
        }
    }

    public class Person : DynamicModel
    {
        public Person()
            : this(new { })
        {

        }

        public Person(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return
            new Presence("FirstName") { ErrorMessage = "First name is required." };

            yield return
            new Confirmation("Email") { ErrorMessage = "Emails must match." };
        }

        public string UpperCaseFirstName()
        {
            return GetMember("FirstName").ToUpper();
        }
    }
}
