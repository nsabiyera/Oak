using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests
{
    class describe_DynamicModels : nspec
    {
        dynamic models;

        bool resultForAny;

        IEnumerable<dynamic> resultForWhere;

        object resultForFirst;

        void before_each()
        {
            models = new DynamicModels(new List<Gemini>());
        }

        void describe_Any()
        {
            context["matching single property"] = () =>
            {
                act = () => resultForAny = models.Any(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["any returns false"] = () => resultForAny.should_be_false();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane" }));

                    it["any returns true"] = () => resultForAny.should_be_true();
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultForAny = models.Any(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 15 }));

                    it["any returns true"] = () => resultForAny.should_be_true();
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 10 }));

                    it["any returns false"] = () => resultForAny.should_be_false();
                };
            };
        }

        void describe_Where()
        {
            context["matching single property"] = () =>
            {
                act = () => resultForWhere = models.Where(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["result list is empty"] = () => resultForWhere.should_be_empty();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane" }));

                    it["returns item"] = () => resultForWhere.Count().should_be(1);
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultForWhere = models.Where(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 15 }));

                    it["returns item"] = () => resultForWhere.Count().should_be(1);
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 10 }));

                    it["result list is empty"] = () => resultForWhere.should_be_empty();
                };
            };
        }

        void describe_First()
        {
            act = () => resultForFirst = models.First(new { Name = "Jane" });

            context["no items in list"] = () =>
            {
                it["result is null"] = () => resultForFirst.should_be(null);
            };

            context["items exist in list that match"] = () =>
            {
                before = () => models.Models.Add(new Gemini(new { Name = "Jane" }));

                it["returns item"] = () => resultForFirst.should_not_be_null();
            };
        }
    }
}
