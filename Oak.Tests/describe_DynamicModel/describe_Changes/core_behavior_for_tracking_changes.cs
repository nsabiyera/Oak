using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.ComponentModel;

namespace Oak.Tests.describe_DynamicModel.describe_Changes
{
    public class MixInChanges
    {
        DynamicModel @this;

        IDictionary<string, object> originalValues;

        IDictionary<string, object> currentValues;

        public MixInChanges(DynamicModel dynamicModel)
        {
            @this = dynamicModel;

            originalValues = new Dictionary<string, object>(dynamicModel.Hash());

            currentValues = dynamicModel.Hash();

            dynamicModel.Virtual.SetMember("IsChanged", new DynamicFunction(IsChanged));

            dynamicModel.Virtual.SetMember("Original", new Func<string, dynamic>(Original));

            dynamicModel.Virtual.SetMember("Changes", new Func<IDictionary<string, dynamic>>(Changes));

            dynamicModel.Virtual.SetMember("ChangesFor", new Func<string, dynamic>(ChangesFor));
        }

        public IDictionary<string, dynamic> Changes()
        {
            var dictionary = new Dictionary<string, dynamic>();

            foreach (var key in currentValues.Keys) dictionary.Add(key, ChangesFor(key));

            return dictionary;
        }

        public dynamic ChangesFor(string property)
        {
            return BeforeAfter(Original(property), currentValues[property]);
        }

        private dynamic BeforeAfter(dynamic before, dynamic after)
        {
            return new { Original = before, New = after };
        }

        public dynamic Original(string property)
        {
            if (!originalValues.ContainsKey(property)) return null;

            return originalValues[property];
        }

        public dynamic IsChanged()
        {
            foreach (var item in currentValues) if (Original(item.Key) != item.Value) return true;

            return false;
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
    }

    class core_behavior_for_tracking_changes : nspec
    {
        dynamic person;

        void tracking_changes_after_new_instantiation_of_model()
        {
            act = () =>
            {
                person = new Person();

                new MixInChanges(person);
            };

            it["has no changes"] = () => ((bool)person.IsChanged()).should_be_false();

            it["methods added are virtual, so that they are ignored by persistance"] = () =>
            {
                ((bool)person.Virtual.RespondsTo("IsChanged")).should_be_true();

                ((bool)person.Virtual.RespondsTo("Original")).should_be_true();

                ((bool)person.Virtual.RespondsTo("Changes")).should_be_true();

                ((bool)person.Virtual.RespondsTo("ChangesFor")).should_be_true();
            };

            context["changing dynamic model"] = () =>
            {
                act = () => person.FirstName = "Jane";

                it["original values are unchanged"] = () => (person.Original("FirstName") as object).should_be(null);

                it["has changes"] = () => ((bool)person.IsChanged()).should_be_true();

                it["lists changes"] = () =>
                {
                    IDictionary<string, dynamic> changes = person.Changes();

                    (changes["FirstName"].Original as object).should_be(null);
                    (changes["FirstName"].New as object).should_be("Jane");
                };

                it["gives changes for property"] = () =>
                {
                    var changes = person.ChangesFor("FirstName");

                    (changes.Original as object).should_be(null);
                    (changes.New as object).should_be("Jane");
                };
            };
        }

        void tracking_changes_when_model_is_initialized_with_dto()
        {
            act = () =>
            {
                person = new Person(new { FirstName = "Jane" });

                new MixInChanges(person);
            };

            it["has no changes"] = () => ((bool)person.IsChanged()).should_be_false();

            context["changing dynamic model"] = () =>
            {
                act = () => person.FirstName = "New Value";

                it["has changes"] = () => ((bool)person.IsChanged()).should_be_true();

                it["lists changes"] = () =>
                {
                    IDictionary<string, dynamic> changes = person.Changes();

                    (changes["FirstName"].Original as object).should_be("Jane");
                    (changes["FirstName"].New as object).should_be("New Value");
                };

                it["gives changes for property"] = () =>
                {
                    var changes = person.ChangesFor("FirstName");

                    (changes.Original as object).should_be("Jane");
                    (changes.New as object).should_be("New Value");
                };
            };
        }
    }
}
