using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.ComponentModel;
using Oak.Tests.describe_DynamicModel.describe_Changes.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Changes
{
    class changes_core_behavior_for_static_object : core_behavior_for_tracking_changes
    {
        public override dynamic NewPerson(object dto)
        {
            return new PersonWithAutoProps(dto).InitializeExtensions();
        }

        void specify_gettable_properties_are_not_tracked_for_changes()
        {
            var person = NewPerson(new { });

            new Changes(person);

            IDictionary<string, dynamic> original = person.Original();

            original.ContainsKey("OnlyGettable").should_be_false();
        }
    }

    class changes_core_behavior_for_dyanmic_object : core_behavior_for_tracking_changes
    {
        public override dynamic NewPerson(object dto)
        {
            return new Person(dto);
        }

        void change_tracking_can_be_added_directly_to_gemini()
        {
            before = () =>
            {
                gemini = new Gemini(new { FirstName = "Amir" });

                new Changes(gemini);
            };

            it["change tracking methods exist when changes is mixed in"] = () =>
                ((bool)gemini.HasChanged()).should_be(false);
        }

        void deleting_a_property()
        {
            act = () =>
            {
                person = new Person(new { FirstName = "Jane" });

                person.DeleteMember("FirstName");
            };

            it["has changes"] = () => ((bool)person.HasChanged()).should_be_true();

            it["lists changes"] = () =>
            {
                IDictionary<string, dynamic> changes = person.Changes();

                (changes["FirstName"].Original as object).should_be("Jane");
                (changes["FirstName"].New as object).should_be(null);
            };

            context["different property is added"] = () =>
            {
                act = () => person.SetMember("LastName", "");

                it["has changes"] = () => ((bool)person.HasChanged()).should_be_true();
            };
        }
    }
    
    abstract class core_behavior_for_tracking_changes : nspec
    {
        public dynamic person;

        public dynamic gemini;

        public abstract dynamic NewPerson(object dto);

        void tracking_changes_after_new_instantiation_of_model()
        {
            act = () =>
            {
                person = NewPerson(null);
            };

            it["has no changes"] = () => ((bool)person.HasChanged()).should_be_false();

            it["property has no changes"] = () => ((bool)person.HasChanged("FirstName")).should_be_false();

            it["methods added are untracked, so that they are ignored by persistance"] = () =>
            {
                var keys = (person.HashExcludingDelegates() as IDictionary<string, object>).Keys.ToList();

                keys.should_not_contain("HasChanged");

                keys.should_not_contain("Original");

                keys.should_not_contain("Changes");
            };

            context["changing dynamic model"] = () =>
            {
                act = () => person.FirstName = "Jane";

                it["original values are unchanged"] = () => (person.Original("FirstName") as object).should_be(null);

                it["has changes"] = () => ((bool)person.HasChanged()).should_be_true();

                it["lists changes"] = () =>
                {
                    dynamic changes = person.Changes();

                    (changes.FirstName.Original as object).should_be(null);
                    (changes.FirstName.New as object).should_be("Jane");
                };

                it["gives changes for property"] = () =>
                {
                    var changes = person.Changes("FirstName");

                    (changes.Original as object).should_be(null);
                    (changes.New as object).should_be("Jane");
                };
            };
        }

        void tracking_changes_when_model_is_initialized_with_dto()
        {
            act = () =>
            {
                person = NewPerson(new { FirstName = "Jane" });
            };

            it["has no changes"] = () => ((bool)person.HasChanged()).should_be_false();

            it["list of changes is empty"] = () => ((int)(person.Changes() as IDictionary<string, object>).Count).should_be(0);

            context["changing dynamic model"] = () =>
            {
                act = () => person.FirstName = "New Value";

                it["has changes"] = () => ((bool)person.HasChanged()).should_be_true();

                it["lists changes"] = () =>
                {
                    IDictionary<string, dynamic> changes = person.Changes();

                    (changes["FirstName"].Original as object).should_be("Jane");
                    (changes["FirstName"].New as object).should_be("New Value");
                };

                it["gives changes for property"] = () =>
                {
                    var changes = person.Changes("FirstName");

                    (changes.Original as object).should_be("Jane");
                    (changes.New as object).should_be("New Value");
                };
            };

            context["setting dynamic model property to what it already is"] = () =>
            {
                act = () => person.FirstName = new string("Jane".ToArray());

                it["isn't considered a change"] = () => ((bool)person.HasChanged()).should_be_false();
            };
        }
    }
}
