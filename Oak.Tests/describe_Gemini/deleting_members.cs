using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using System.Reflection;
using Oak.Tests.describe_Gemini.Classes;

namespace Oak.Tests.describe_Gemini
{
    class deleting_members : _describe_Gemini
    {
        void given_a_member_is_defined()
        {
            before = () => Gemini().RespondsTo("Title").should_be_true();

            act = () => Gemini().DeleteMember("Title");

            it["no longer responds to member"] = () => Gemini().RespondsTo("Title").should_be_false();
        }

        void deleting_members_by_case()
        {
            new[] { "title", "TITLE" }.Do(member =>
            {
                context["member deletion is case insensitive ({0})".With(member)] = () =>
                {
                    before = () => Gemini().RespondsTo("Title").should_be_true();

                    act = () => Gemini().DeleteMember(member);

                    it["no longer responds to member"] = () => Gemini().RespondsTo("Title").should_be_false();
                };
            });
        }

        void member_is_not_defined()
        {
            before = () => Gemini().RespondsTo("FooBar").should_be_false();

            act = () => Gemini().DeleteMember("FooBar");

            it["ignores deletion"] = () => Gemini().RespondsTo("FooBar").should_be_false();
        }
    }
}
