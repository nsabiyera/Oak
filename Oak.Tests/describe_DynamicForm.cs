using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.SampleClasses;

namespace Oak.Tests
{
    class RegularClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class RegularPrototype : Prototype
    {
        public RegularPrototype(string name)
            : base(new { Name = name })
        {

        }
    }

    class NestedPrototype : Prototype
    {
        public NestedPrototype(object o)
            : base(o)
        {

        }

        public string AllCaps
        {
            get
            {
                return (Expando.Name as string).ToUpper();
            }
        }
    }


    class describe_DynamicForm : nspec
    {
        dynamic form;
        dynamic entity;
        ElementMetaData result;

        void creating_element_meta_data_for_a_regular_class()
        {
            before = () =>
            {
                entity = new RegularClass
                {
                    Name = "Jane Doe",
                    Age = 10
                };

                form = new DynamicForm(entity);
            };

            context["property being converted is a string"] = () =>
            {
                act = () => result = form.Name;

                it["the Value property is set to the class's property value"] = () =>
                    result.Value.should_be(entity.Name as string);

                it["the Id property is set to the class's "] = () =>
                    result.Id.should_be("Name");
            };

            context["property being converted is an int"] = () =>
            {
                act = () => result = form.Age;

                it["the Value property is set to the class's property value"] = () =>
                    result.Value.should_be((int)entity.Age);
            };
        }

        void creating_element_meta_data_for_a_prototype()
        {
            before = () =>
            {
                entity = new RegularPrototype("Jane Doe");

                form = new DynamicForm(entity);
            };

            context["property being converted is a string"] = () =>
            {
                act = () => result = form.Name;

                it["the Value property is set to the prototype's property value"] = () =>
                    result.Value.should_be(entity.Name as string);
            };
        }

        void creating_element_meta_data_for_a_nested_prototype()
        {
            before = () =>
            {
                entity = new NestedPrototype(new RegularPrototype("jane"));

                form = new DynamicForm(entity);
            };

            context["property being converted is a newly defined property on top level prototype"] = () =>
            {
                act = () => result = form.AllCaps;

                it["the Value property is set to the newly defined property value"] = () =>
                    result.Value.should_be("JANE");
            };
        }

        void creating_element_meta_data_for_a_dynamic_model()
        {
            before = () =>
            {
                entity = new Person();

                entity.Email = "user@example.com";

                entity.EmailConfirmation = "user@example.com";

                form = new DynamicForm(entity);
            };

            context["property is a property that has been added via validation constraint"] = () =>
            {
                act = () => result = form.Email;

                it["the Value property is set to the property defined by the validation constraint"] = () =>
                {
                    result.Value.should_be("user@example.com");
                };
            };

            context["property is a property that has been added as a virtual property"] = () =>
            {
                act = () => result = form.EmailConfirmation;

                it["the Value property is set to the virtual property defined by the validation constraint"] = () =>
                {
                    result.Value.should_be("user@example.com");
                };
            };
        }

        void accessing_a_property_that_doesnt_exist_on_a_regular_class()
        {
            before = () => form = new DynamicForm(new RegularClass());

            it["throws a friendly exception"] =
                expect<InvalidOperationException>("The entity that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName);
        }

        void accessing_a_property_that_doesnt_exist_on_a_Prototype()
        {
            before = () => form = new DynamicForm(new RegularPrototype("jane"));

            it["throws a friendly exception"] =
                expect<InvalidOperationException>("The Prototype that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName);
        }

        void concatenating_html_attributes()
        {
            before = () =>
            {
                entity = new DynamicModel();

                entity.Expando.Title = "Some Title";

                form = new DynamicForm(entity);
            };

            it["retains value of dynamic model"] = () => (form.Title() as ElementMetaData).Value.should_be("Some Title");

            it["throws a friendly exception when constructing element meta data for a property that doesn't exist"] =
                expect<InvalidOperationException>("The Prototype that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName());

            context["adding reserved dictionary values"] = () =>
            {
                act = () =>
                    result = form.Title(new Hash
                    {
                        { FirstElementAttribute(), "firstName" }
                    });

                it["key value is marked as an element attribute"] = () =>
                    result.Attributes[FirstElementAttribute()].should_be("firstName");
            };

            context["adding unreserved dictionary values"] = () =>
            {
                act = () =>
                    result = form.Title(new Hash
                    {
                        { "background-color", "red" }
                    });

                it["key value is regarded as a style entry"] = () =>
                    result.Styles["background-color"].should_be("red");
            };
        }

        string FirstElementAttribute()
        {
            return (form as DynamicForm).ElementAttributes().First();
        }
    }
}
