using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests
{
    class RegularClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class RegularGemini : Gemini
    {
        public RegularGemini(string name)
            : base(new { Name = name })
        {

        }
    }

    class NestedGemini : Gemini
    {
        public NestedGemini(object o)
            : base(o)
        {

        }

        public string AllCaps
        {
            get
            {
                return (Prototype.Name as string).ToUpper();
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
                    result.Value().should_be(entity.Name as string);

                it["the Id property is set to the class's "] = () =>
                    result.Id().should_be("Name");
            };

            context["property being converted is an int"] = () =>
            {
                act = () => result = form.Age;

                it["the Value property is set to the class's property value"] = () =>
                    result.Value().should_be(entity.Age.ToString() as string);
            };
        }

        void creating_element_meta_data_for_a_gemini()
        {
            before = () =>
            {
                entity = new RegularGemini("Jane Doe");

                form = new DynamicForm(entity);
            };

            context["property being converted is a string"] = () =>
            {
                act = () => result = form.Name;

                it["the Value property is set to the gemini's property value"] = () =>
                    result.Value().should_be(entity.Name as string);
            };
        }

        void creating_element_meta_data_for_a_nested_gemini()
        {
            before = () =>
            {
                entity = new NestedGemini(new RegularGemini("jane"));

                form = new DynamicForm(entity);
            };

            context["property being converted is a newly defined property on top level gemini"] = () =>
            {
                act = () => result = form.AllCaps;

                it["the Value property is set to the newly defined property value"] = () =>
                    result.Value().should_be("JANE");
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
                    result.Value().should_be("user@example.com");
                };

                it["value is not considered as part of the element meta data hash (used by OakForm.cshtml)"] = () => result.Hash.ContainsKey("value").should_be_false();
            };

            context["property is a property that has been added as a virtual property"] = () =>
            {
                act = () => result = form.EmailConfirmation;

                it["the Value property is set to the virtual property defined by the validation constraint"] = () =>
                {
                    result.Value().should_be("user@example.com");
                };
            };
        }

        void creating_element_data_out_of_hash()
        {
            before = () =>
            {
                result = new ElementMetaData(new Hash
                {
                    { "id", "Title" },
                    { "value", "Some Value" },
                    { "data-foo", "foobar" }
                });
            };

            it["creates element metadata"] = () =>
            {
                result.Id().should_be("Title");

                result.Value().should_be("Some Value");

                result.Hash.First().Value.should_be("foobar");
            };
        }

        void creating_element_meta_data_out_of_constructor_initializer()
        {
            context["name is specified"] = () =>
            {
                before = () =>
                {
                    result = new ElementMetaData("Title");
                };

                it["creates element metadata"] = () =>
                {
                    result.Id().should_be("Title");
                };
            };

            context["name and value are specified"] = () =>
            {
                before = () =>
                {
                    result = new ElementMetaData("Title", "Some Value");
                };

                it["creates element metadata"] = () =>
                {
                    result.Id().should_be("Title");

                    result.Value().should_be("Some Value");
                };
            };
        }

        void accessing_a_property_that_doesnt_exist_on_a_regular_class()
        {
            before = () => form = new DynamicForm(new RegularClass());

            it["throws a friendly exception"] =
                expect<InvalidOperationException>("The entity that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName);
        }

        void accessing_a_property_that_doesnt_exist_on_a_Gemini()
        {
            before = () => form = new DynamicForm(new RegularGemini("jane"));

            it["throws a friendly exception"] =
                expect<InvalidOperationException>("The Gemini that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName);
        }

        void concatenating_html_attributes()
        {
            before = () =>
            {
                entity = new DynamicModel();

                entity.Prototype.Title = "Some Title";

                form = new DynamicForm(entity);
            };

            it["retains value of dynamic model"] = () => (form.Title() as ElementMetaData).Value().should_be("Some Title");

            it["throws a friendly exception when constructing element meta data for a property that doesn't exist"] =
                expect<InvalidOperationException>("The Gemini that you passed into DynamicForm does not contain the property called LastName.", () => result = form.LastName());

            context["adding id override"] = () =>
            {
                act = () =>
                    result = form.Title(new Hash
                    {
                        { "id", "new-title" }
                    });

                it["the id is overridden with the one specified"] = () =>
                    result.Id().should_be("new-title");

                it["id property should not be contained in hash (used by OakForm.cshtml)"] = () => result.Hash.ContainsKey("id").should_be_false();
            };

            context["added value attribute"] = () =>
            {
                act = () =>
                    result = form.Title(new Hash
                    {
                        { "value", "some default" }
                    });

                context["Title property has value"] = () =>
                {
                    before = () => entity.Title = "A Title";

                    it["the title is set to the entity's title"] = () => result.Value().should_be("A Title");
                };

                context["Title property doesn't have a value"] = () =>
                {
                    before = () => entity.Title = null;

                    it["the title is set to the value from the hash"] = () => result.Value().should_be("some default");
                };
            };
        }
    }
}
