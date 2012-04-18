using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class gemini_dynamic_models : _dynamic_models
    {
        void describe_Any()
        {
            before = () => models = new DynamicModels(new List<Gemini>());

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

            context["any with no parameters just checks count (which is currently 0)"] = () =>
            {
                act = () => resultForAny = models.Any();

                it["return false"] = () => resultForAny.should_be_false();
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

        void describe_Count()
        {
            before = () => models = new DynamicModels(new List<Gemini>());

            it["reports count"] = () => ((int)models.Count()).should_be(0);
        }

        void describe_Where()
        {
            before = () => models = new DynamicModels(new List<Gemini>());

            context["matching single property"] = () =>
            {
                act = () => resultList = models.Where(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["result list is empty"] = () => resultList.should_be_empty();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane" }));

                    it["returns item"] = () => resultList.Count().should_be(1);
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultList = models.Where(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 15 }));

                    it["returns item"] = () => resultList.Count().should_be(1);
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () => models.Models.Add(new Gemini(new { Name = "Jane", Age = 10 }));

                    it["result list is empty"] = () => resultList.should_be_empty();
                };
            };
        }

        void describe_First()
        {
            before = () => models = new DynamicModels(new List<Gemini>());

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

        void describe_OrderBy()
        {
            before = () =>
            {
                models = new DynamicModels(new List<Gemini>());

                models.Models.Add(new Gemini(new { Value1 = "C", Value2 = "X" }));

                models.Models.Add(new Gemini(new { Value1 = "B", Value2 = "Y" }));

                models.Models.Add(new Gemini(new { Value1 = "D", Value2 = "Y" }));

                models.Models.Add(new Gemini(new { Value1 = "A", Value2 = "X" }));
            };

            it["orders records ascending"] = () =>
            {
                resultList = models.OrderBy(new { Value1 = "asc" });

                (resultList.First().Value1 as string).should_be("A");
            };

            it["orders records descending"] = () =>
            {
                resultList = models.OrderBy(new { Value1 = "desc" });

                (resultList.First().Value1 as string).should_be("D");
            };

            it["orders records based on multiple ascending specifications"] = () =>
            {
                var orderBy = (
                    models.OrderBy(new
                    {
                        Value2 = "asc",
                        Value1 = "asc",
                    }) as IEnumerable<dynamic>).ToList();

                (orderBy[0].Value1 as string).should_be("A");

                (orderBy[0].Value2 as string).should_be("X");

                (orderBy[1].Value2 as string).should_be("X");

                (orderBy[1].Value1 as string).should_be("C");
            };

            it["orders records based on multiple descending specifications"] = () =>
            {
                var orderBy = (
                    models.OrderBy(new
                    {
                        Value2 = "desc",
                        Value1 = "desc"
                    }) as IEnumerable<dynamic>).ToList();

                (orderBy[0].Value1 as string).should_be("D");

                (orderBy[0].Value2 as string).should_be("Y");

                (orderBy[1].Value1 as string).should_be("B");

                (orderBy[1].Value2 as string).should_be("Y");
            };
        }
    }
}
