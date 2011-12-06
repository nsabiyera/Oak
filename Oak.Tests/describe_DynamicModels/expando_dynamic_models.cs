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

            context["chaining wheres"] = () =>
            {
                before = () =>
                {
                    models.Models.Add(new Gemini(new { LastName = "Smith", FirstName = "Jane" }));

                    models.Models.Add(new Gemini(new { LastName = "Smith", FirstName = "John" }));

                    models.Models.Add(new Gemini(new { LastName = "Doe", FirstName = "John" }));
                };

                act = () => resultForWhere = models.Where(new { LastName = "Smith" }).Where(new { FirstName = "Jane" });

                it["applies first filter then next filter"] = () => resultForWhere.Count().should_be(1);
            };
        }

        void describe_First()
        {
            context["first taking in a 'where clause'"] = () =>
            {
                before = () => models = new DynamicModels(new List<ExpandoObject>());

                act = () => resultForFirst = models.First(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["result is null"] = () => resultForFirst.should_be(null);
                };

                context["items exist in list that match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane" }));

                    it["returns item"] = () => ((string)((dynamic)resultForFirst).Name).should_be("Jane");
                };
            };

            context["first doesn't take in a 'where clause'"] = () =>
            {
                before = () =>
                {
                    models = new DynamicModels(new List<ExpandoObject>());

                    models.Models.Add(new Gemini(new { Name = "Jane" }));
                };

                act = () => resultForFirst = models.First();

                it["returns first record"] = () => ((string)((dynamic)resultForFirst).Name).should_be("Jane");
            };
        }
    }
}
