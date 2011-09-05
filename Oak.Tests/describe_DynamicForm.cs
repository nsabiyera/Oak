using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests
{
    class RegularClass
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    class RegularMix : Mix
    {
        public RegularMix(string name)
            : base(new { Name = name })
        {

        }
    }

    class NestedMix : Mix
    {
        public NestedMix(object mixWith)
            : base(mixWith)
        {

        }

        public string AllCaps
        {
            get
            {
                return (MixWith.Name as string).ToUpper();
            }
        }
    }


    class describe_DynamicForm : nspec
    {
        dynamic dynamicForm;
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

                dynamicForm = new DynamicForm(entity);
            };

            context["property being converted is a string"] = () =>
            {
                act = () => result = dynamicForm.Name;

                it["the Value property is set to the class's property value"] = () =>
                    result.Value.should_be(entity.Name as string);

                it["the Id property is set to the class's "] = () =>
                    result.Id.should_be("Name");
            };

            context["property being converted is an int"] = () =>
            {
                act = () => result = dynamicForm.Age;

                it["the Value property is set to the class's property value"] = () =>
                    result.Value.should_be((int)entity.Age);
            };
        }

        void creating_element_meta_data_for_a_mix()
        {
            before = () =>
            {
                entity = new RegularMix("Jane Doe");

                dynamicForm = new DynamicForm(entity);
            };

            context["property being converted is a string"] = () =>
            {
                act = () => result = dynamicForm.Name;

                it["the Value property is set to the mix's property value"] = () =>
                    result.Value.should_be(entity.Name as string);
            };
        }

        void creating_element_meta_data_for_a_nested_mix()
        {
            before = () =>
            {
                entity = new NestedMix(new RegularMix("jane"));

                dynamicForm = new DynamicForm(entity);
            };

            context["proeprty being converted is a newly defined property on top level mix"] = () =>
            {
                act = () => result = dynamicForm.AllCaps;

                it["the Value property is set to the newly defined property value"] = () =>
                    result.Value.should_be("JANE");
            };
        }

        void accessing_a_property_that_doesnt_exist_on_a_regular_class()
        {
            before = () => dynamicForm = new DynamicForm(new RegularClass());

            it["throws an friendly exception"] =
                expect<InvalidOperationException>("The entity that you passed into DynamicForm does not contain the property called LastName.", () => result = dynamicForm.LastName);
        }

        void accessing_a_property_that_doesnt_exist_on_a_Mix()
        {
            before = () => dynamicForm = new DynamicForm(new RegularMix("jane"));

            it["throws an friendly exception"] =
                expect<InvalidOperationException>("The Mix that you passed into DynamicForm does not contain the property called LastName.", () => result = dynamicForm.LastName);
        }

        void concatenating_html_attributes()
        {
            before = () =>
            {
                entity = new DynamicModel();

                dynamicForm = new DynamicForm(entity);
            };

            act = () =>
            {
                result = dynamicForm.Title(new Dictionary<string, string>
                                            {
                                                {"id", "name" }
                                            });
            };

            result.should_not_be_null();


        }
    }
}
