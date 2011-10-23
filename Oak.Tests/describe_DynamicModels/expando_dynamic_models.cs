using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;

namespace Oak.Tests.describe_DynamicModels
{
    class expando_dynamic_models : _dynamic_models
    {
        void describe_Any()
        {
            before = () => models = new DynamicModels(new List<ExpandoObject>());

            context["matching single property"] = () =>
            {
                act = () => resultForAny = models.Any(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["any returns false"] = () => resultForAny.should_be_false();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        models.Models.Add(expando);
                    };

                    it["any returns true"] = () => resultForAny.should_be_true();
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultForAny = models.Any(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        expando.Age = 15;
                        models.Models.Add(expando);
                    };

                    it["any returns true"] = () => resultForAny.should_be_true();
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        expando.Age = 10;
                        models.Models.Add(expando);
                    };

                    it["any returns false"] = () => resultForAny.should_be_false();
                };
            };
        }

        void describe_Where()
        {
            before = () => models = new DynamicModels(new List<ExpandoObject>());

            context["matching single property"] = () =>
            {
                act = () => resultForWhere = models.Where(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["result list is empty"] = () => resultForWhere.should_be_empty();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        models.Models.Add(expando);
                    };

                    it["returns item"] = () => resultForWhere.Count().should_be(1);
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultForWhere = models.Where(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        expando.Age = 15;
                        models.Models.Add(expando);
                    };

                    it["returns item"] = () => resultForWhere.Count().should_be(1);
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () =>
                    {
                        dynamic expando = new ExpandoObject();
                        expando.Name = "Jane";
                        expando.Age = 10;
                        models.Models.Add(expando);
                    };

                    it["result list is empty"] = () => resultForWhere.should_be_empty();
                };
            };
        }

        void describe_First()
        {
            before = () => models = new DynamicModels(new List<ExpandoObject>());

            act = () => resultForFirst = models.First(new { Name = "Jane" });

            context["no items in list"] = () =>
            {
                it["result is null"] = () => resultForFirst.should_be(null);
            };

            context["items exist in list that match"] = () =>
            {
                before = () =>
                {
                    dynamic expando = new ExpandoObject();
                    expando.Name = "Jane";
                    models.Models.Add(expando);
                };

                it["returns item"] = () => resultForFirst.should_not_be_null();
            };
        }
    }
}
