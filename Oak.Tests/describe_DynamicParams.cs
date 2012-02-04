using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Collections.Specialized;

namespace Oak.Tests
{
    class describe_DynamicParams : nspec
    {
        DynamicParams dynamicParams;

        NameValueCollection nameValueCollection;

        dynamic asDynamic;

        void before_each()
        {
            nameValueCollection = new NameValueCollection();
        }

        void act_each()
        {
            dynamicParams = new DynamicParams(nameValueCollection, null);

            asDynamic = dynamicParams;
        }

        void describe_casting_assumptions()
        {
            context["evaluating form collection for potential Int id candidate"] = () =>
            {
                context["name ends in Id"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonId", "123");

                    it["comparing key's value with an int passes"] = () =>
                        ((int)asDynamic.PersonId).should_be(123);
                };

                context["name ends in Id but isn't an int"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonId", "123Foobar");

                    it["keeps original value"] = () =>
                        ((string)asDynamic.PersonId).should_be("123Foobar");
                };

                context["values with leading zero's is supplied"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonId", "000123");

                    it["converts to int"] = () =>
                        ((int)asDynamic.PersonId).should_be(123);
                };

                context["name ends in ID"] = () => 
                {
                    before = () => nameValueCollection.Add("PersonID", "123");

                    it["comparing key's value with an int passes"] = () =>
                        ((int)asDynamic.PersonID).should_be(123);
                };

                context["name contains ID"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonIDFoobar", "123");

                    it["disregards conversion"] = () =>
                        (asDynamic.PersonIDFoobar as string).should_be("123");
                };
            };

            context["evaluating form collection for potential Guid id candidate"] = () =>
            {
                context["name ends in Id"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonId", Guid.Empty.ToString());

                    it["comparing key's value with a guid passes"] = () =>
                        ((Guid)asDynamic.PersonId).should_be(Guid.Empty);
                };

                context["name ends in ID"] = () =>
                {
                    before = () => nameValueCollection.Add("PersonID", Guid.Empty.ToString());

                    it["comparing key's value with a guid passes"] = () =>
                        ((Guid)asDynamic.PersonId).should_be(Guid.Empty);
                };
            };
        }
    }
}
