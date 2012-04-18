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

                models.Models.Add(new Gemini(new { FirstName = "Jane", LastName = "Doe" }));

                models.Models.Add(new Gemini(new { FirstName = "Bob", LastName = "Smith" }));

                models.Models.Add(new Gemini(new { FirstName = "John", LastName = "Smith" }));

                models.Models.Add(new Gemini(new { FirstName = "Andy", LastName = "Doe" }));
            };

            it["orders records ascending"] = () =>
            {
                resultList = models.OrderBy(new { FirstName = "asc" });

                (resultList.First().FirstName as string).should_be("Andy");
            };

            it["orders records descending"] = () =>
            {
                resultList = models.OrderBy(new { FirstName = "desc" });

                (resultList.First().FirstName as string).should_be("John");
            };

            it["orders records based on multiple ascending specifications"] = () =>
            {
                var orderBy = (
                    models.OrderBy(new
                    {
                        LastName = "asc",
                        FirstName = "asc",
                    }) as IEnumerable<dynamic>).ToList();

                (orderBy[0].LastName as string).should_be("Doe");

                (orderBy[0].FirstName as string).should_be("Andy");

                (orderBy[1].LastName as string).should_be("Doe");

                (orderBy[1].FirstName as string).should_be("Jane");
            };

            it["orders records based on multiple descending specifications"] = () =>
            {
                var orderBy = (
                    models.OrderBy(new
                    {
                        LastName = "desc",
                        FirstName = "desc"
                    }) as IEnumerable<dynamic>).ToList();

                (orderBy[0].LastName as string).should_be("Smith");

                (orderBy[0].FirstName as string).should_be("John");

                (orderBy[1].LastName as string).should_be("Smith");

                (orderBy[1].FirstName as string).should_be("Bob");
            };
        }
    }
}
