using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;
using Oak.Tests.describe_DynamicModel.describe_Association.Classes;

namespace Oak.Tests.describe_DynamicModel
{
    class core_behavior_for_dynamic_model : nspec
    {
        dynamic loan;

        IEnumerable<string> methods;

        void before_each()
        {
            loan = new Loan();
        }

        void root_properties()
        {
            context["adding properties to dynamic model"] = () =>
            {
                act = () => loan.Term = 5;

                it["the property is defined on dynamic model"] = () => ((int)loan.Term).should_be(5);

                it["property exists as a root property"] = () => ((int)Properties().Term).should_be(5);

                it["responds to newly defined property"] = () => (loan as DynamicModel).RespondsTo("Term").should_be_true();
            };
        }

        void root_methods()
        {
            context["adding methods to dynamic model"] = () =>
            {
                act = () => loan.MonthsFor = new Func<double, double>(months => months * 12.0);

                it["the property is defined on dynamic model"] = () => ((double)loan.MonthsFor(1)).should_be(12);

                it["method exists on root"] = () => ((double)Properties().MonthsFor(1)).should_be(12);

                it["responds to newly defined method"] = () => (loan as DynamicModel).RespondsTo("MonthsFor").should_be_true();
            };
        }

        void redefining_methods()
        {
            context["method is redefined"] = () =>
            {
                act = () => loan.Customer = new DynamicFunction(() => new { Name = "Jane Doe" });

                it["behaves using redefined behavior"] = () => (loan.Customer().Name as string).should_be("Jane Doe");

                it["exists replaces virtual method"] = () => (Properties().Customer().Name as string).should_be("Jane Doe");

                it["responds to redefined method"] = () => Model().RespondsTo("Customer").should_be_true();
            };
        }

        void describe_method_list()
        {
            context["dynamic model has methods and properties defined on root and virtual level"] = () =>
            {
                before = () =>
                {
                    Model().RespondsTo("Customer").should_be_true();
                    loan.Term = 5;
                };

                act = () => methods = Model().Methods();

                it["contains both defined methods"] = () =>
                {
                    methods.should_contain("Customer");
                    methods.should_contain("Term");
                };
            };
        }

        void deleting_members()
        {
            context["dynamic model has methods and properties definded on root and virtual level"] = () =>
            {
                before = () =>
                {
                    Model().RespondsTo("Customer").should_be_true();
                    loan.Term = 5;
                };

                act = () =>
                {
                    Model().Methods().ToList().Do(s => Model().DeleteMember(s));
                    methods = Model().Methods();
                };

                it["both methods are deleted"] = () =>
                {
                    methods.ToList().should_not_contain("Customer");
                    methods.ToList().should_not_contain("Term");
                };
            };
        }

        dynamic Properties()
        {
            return Model().Expando;
        }

        DynamicModel Model()
        {
            return loan as DynamicModel;
        }
    }
}
