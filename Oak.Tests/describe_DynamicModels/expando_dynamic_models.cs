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
            before = () => models = new DynamicModels(new List<Prototype>());

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
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        models.Models.Add(prototype);
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
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        prototype.Age = 15;
                        models.Models.Add(prototype);
                    };

                    it["any returns true"] = () => resultForAny.should_be_true();
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () =>
                    {
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        prototype.Age = 10;
                        models.Models.Add(prototype);
                    };

                    it["any returns false"] = () => resultForAny.should_be_false();
                };
            };
        }

        void describe_Where()
        {
            before = () => models = new DynamicModels(new List<Prototype>());

            context["matching single property"] = () =>
            {
                act = () => resultList = models.Where(new { Name = "Jane" });

                context["no items in list"] = () =>
                {
                    it["result list is empty"] = () => resultList.should_be_empty();
                };

                context["items exist in list that match"] = () =>
                {
                    before = () =>
                    {
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        models.Models.Add(prototype);
                    };

                    it["returns item"] = () => resultList.Count().should_be(1);
                };
            };

            context["item exists in list that match multiple properties"] = () =>
            {
                act = () => resultList = models.Where(new { Name = "Jane", Age = 15 });

                context["entry exists where all properties match"] = () =>
                {
                    before = () =>
                    {
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        prototype.Age = 15;
                        models.Models.Add(prototype);
                    };

                    it["returns item"] = () => resultList.Count().should_be(1);
                };

                context["entry exists where all properties do not match"] = () =>
                {
                    before = () =>
                    {
                        dynamic prototype = new Prototype();
                        prototype.Name = "Jane";
                        prototype.Age = 10;
                        models.Models.Add(prototype);
                    };

                    it["result list is empty"] = () => resultList.should_be_empty();
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

                act = () => resultList = models.Where(new { LastName = "Smith" }).Where(new { FirstName = "Jane" });

                it["applies first filter then next filter"] = () => resultList.Count().should_be(1);
            };

            context["value being compared is a dynamic function"] = () =>
            {
                before = () =>
                {
                    models.Models.Add(
                        new Gemini(new
                        {
                            LastName = new DynamicFunction(() => "Smith"),
                            FirstName = new DynamicFunction(() => "Jane")
                        }));

                    models.Models.Add(
                        new Gemini(new
                        {
                            LastName = new DynamicFunction(() => "Smith"),
                            FirstName = new DynamicFunction(() => "John")
                        }));

                    models.Models.Add(
                        new Gemini(new
                        {
                            LastName = new DynamicFunction(() => "Doe"),
                            FirstName = new DynamicFunction(() => "John")
                        }));
                };

                act = () => { };

                it["applies first filter then next filter"] = () =>
                {
                    resultList = models.Where(new { LastName = "Smith" }).Where(new { FirstName = "Jane" });

                    resultList.Count().should_be(1);
                };
            };
        }

        void describe_ToList()
        {
            before = () =>
            {
                models.Models.Add(new Gemini(new { LastName = "Smith", FirstName = "Jane" }));

                models.Models.Add(new Gemini(new { LastName = "Smith", FirstName = "John" }));

                models.Models.Add(new Gemini(new { LastName = "Doe", FirstName = "John" }));
            };

            it["converts IEnumerable to a list"] = () =>
            {
                var list = models.ToList();

                (list as List<dynamic>).should_contain(models.First() as object);
            };
        }

        object[] listToConvert;
        void describe_ToModels()
        {
            before = () =>
            {
                listToConvert = new object[] 
                {
                    new Gemini(new { LastName = "Smith", FirstName = "Jane" })
                };
            };

            it["converts enumerable to dynamic models"] = () =>
            {
                var list = listToConvert.ToModels();

                (list as object).should_cast_to<DynamicModels>();
            };
        }

        void describe_First()
        {
            context["first taking in a 'where clause'"] = () =>
            {
                before = () => models = new DynamicModels(new List<Prototype>());

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
                    models = new DynamicModels(new List<Prototype>());

                    models.Models.Add(new Gemini(new { Name = "Jane" }));
                };

                act = () => resultForFirst = models.First();

                it["returns first record"] = () => ((string)((dynamic)resultForFirst).Name).should_be("Jane");
            };
        }

        void describe_Last()
        {
            context["last taking in a 'where clause'"] = () =>
            {
                before = () => models = new DynamicModels(new List<Prototype>());

                act = () => resultForFirst = models.Last(new { Name = "Jane" });

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

            context["last doesn't take in a 'where clause'"] = () =>
            {
                before = () =>
                {
                    models = new DynamicModels(new List<Prototype>());

                    models.Models.Add(new Gemini(new { Name = "Jane" }));
                };

                act = () => resultForFirst = models.Last();

                it["returns first record"] = () => ((string)((dynamic)resultForFirst).Name).should_be("Jane");
            };
        }

        void describe_OrderBy()
        {
            context["order by works for prototype objects"] = () =>
            {
                before = () =>
                {
                    models = new DynamicModels(new List<Prototype>());

                    dynamic prototype = new Prototype();
                    prototype.Value = "A";
                    models.Models.Add(prototype);

                    prototype = new Prototype();
                    prototype.Value = "B";
                    models.Models.Add(prototype);
                };

                act = () => resultList = models.OrderBy(new { Value = "desc" });

                it["ordering is applied"] = () => (resultList.First().Value as string).should_be("B");
            };
        }
    }
}
