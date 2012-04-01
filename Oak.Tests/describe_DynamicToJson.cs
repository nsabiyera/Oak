using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using Newtonsoft.Json;

namespace Oak.Tests
{
    public class SomeDynamicModel : DynamicModel
    {
        public SomeDynamicModel(object dto)
            : base(dto)
        {
        }

        public string Name
        {
            get { return "SomeName"; }
        }
    }

    class describe_DynamicToJson : nspec
    {
        dynamic objectToConvert;

        string jsonString;

        void describe_expando_to_json()
        {
            before = () =>
            {
                objectToConvert = new ExpandoObject();
                objectToConvert.Id = 15;
                objectToConvert.String = "hello";
                objectToConvert.Char = 'a';
                objectToConvert.DateTime = DateTime.Today;
                objectToConvert.Double = (double)100;
                objectToConvert.Guid = Guid.Empty;
                objectToConvert.Decimal = (decimal)15;
                objectToConvert.StringAsNull = null as string;
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts expando"] = () =>
                jsonString.should_be(@"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6}, ""StringAsNull"": {7} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15, "null"));
        }

        void describe_gemini_to_json()
        {
            before = () =>
            {

            };

            act = () =>

            it["converts gemini", "wip"] = () => { };
            {
                objectToConvert = new Gemini(new
                {
                    Id = 15,
                    String = "hello",
                    Char = 'a',
                    DateTime = DateTime.Today,
                    Double = (double)100,
                    Guid = Guid.Empty,
                    Decimal = (decimal)15,
                });

                jsonString = DynamicToJson.Convert(objectToConvert);

                jsonString.should_be(@"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15));

            };
        }

        void describe_dynamic_model_to_json()
        {
            before = () =>
            {
                objectToConvert = new DynamicModel(new
                {
                    Id = 15,
                    String = "hello",
                    Char = 'a',
                    DateTime = DateTime.Today,
                    Double = (double)100,
                    Guid = Guid.Empty,
                    Decimal = (decimal)15,
                });
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts dynamic model"] = () =>
                jsonString.should_be(@"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15));
        }

        void describe_collection()
        {
            before = () =>
            {
                dynamic expando = new ExpandoObject();
                expando.Id = 1;

                objectToConvert = new List<dynamic>
                {
                    expando,
                    new DynamicModel(new { Id = 2 }),
                    new Gemini(new { Id = 3 }),
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts collection"] = () => jsonString.should_be(@"[ { ""Id"": 1 }, { ""Id"": 2 }, { ""Id"": 3 } ]");
        }

        void describe_can_be_converted()
        {
            new Each<dynamic, bool, string>()
            {
                { new ExpandoObject(), true, "expando" },
                { new Gemini(), true, "gemini" },
                { new DynamicModel(), true, "dynamic model" },
                { new { Name = "Jane Doe" }, true, "anonymous type" },
                { "Jane Doe", false, "string" },
                { new List<object> { new ExpandoObject(), new Gemini(), new DynamicModel() }, true, "list containing convertable types" },
                { new List<string>() { "Jane", "Doe" }, false, "list of string" },
                { new List<object> { new { Name = "Jane Doe" }, new ExpandoObject(), new Gemini() }, true, "list containing gemini's and anonymous types" },
                { new List<object> { new { Name = "Jane Doe" }, "Foobar" }, false, "list containing convertable types and non convertable types" },
                { new List<string>(), true, "empty list" }
            }.Do((entity, expectedResult, type) =>
            {
                it["{0} should evaluate to: {1}".With(type, expectedResult)] = () =>
                    DynamicToJson.CanConvertObject(entity as object).should_be(expectedResult);
            });
        }

        void describe_anonymous_type_to_json()
        {
            before = () => objectToConvert = new { FirstName = "Jane", LastName = "Doe" };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts properties of anonymous type"] = () =>
                jsonString.should_be(@"{{ ""FirstName"": ""{0}"", ""LastName"": ""{1}"" }}".With("Jane", "Doe"));
        }

        void converting_anonymous_types_that_have_defferred_execution()
        {
            before = () =>
            {
                List<dynamic> users = new List<dynamic>
                {
                    new { Name = "Jane" },
                    new { Name = "John" },
                    new { Name = "Jake" }
                };

                objectToConvert = new
                {
                    Users = users.Where(s => s.Name.StartsWith("Ja"))
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["executes deferred statement and serializes result"] = () =>
            {
                jsonString.should_be(@"{ ""Users"": [ { ""Name"": ""Jane"" }, { ""Name"": ""Jake"" } ] }");
            };
        }

        void coverting_list_string()
        {
            before = () =>
            {
                List<dynamic> users = new List<dynamic>
                {
                    "Jane",
                    "Doe"
                };

                objectToConvert = new
                {
                    Users = users.Where(s => s.Contains("e"))
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["executes deferred statement and serializes result"] = () =>
            {
                jsonString.should_be(@"{ ""Users"": [ ""Jane"", ""Doe"" ] }");
            };
        }

        void converting_list_numeric()
        {
            before = () =>
            {
                List<dynamic> users = new List<dynamic>
                {
                    10,
                    20
                };

                objectToConvert = new
                {
                    Users = users.Where(s => s % 10 == 0)
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["executes deferred statement and serializes result"] = () =>
            {
                jsonString.should_be(@"{ ""Users"": [ 10, 20 ] }");
            };
        }

        void converting_list_of_boolean()
        {
            before = () =>
            {
                List<dynamic> users = new List<dynamic>
                {
                    true,
                    false
                };

                objectToConvert = new
                {
                    IsAdded = true,
                    Users = users.Where(s => s == true || s == false)
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["executes deferred statement and serializes result"] = () =>
            {
                jsonString.should_be(@"{ ""IsAdded"": true, ""Users"": [ true, false ] }");
            };
        }

        void converting_nested_object()
        {
            before = () =>
            {
                objectToConvert = new Gemini(
                new
                {
                    Id = 15,
                    Name = "Mirror's Edge",
                    Owner = new Gemini(new
                    {
                        Id = 22,
                        Handle = "@amirrajan"
                    })
                });
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts whole object graph"] = () =>
            {
                jsonString.should_be(@"{ ""Id"": 15, ""Name"": ""Mirror's Edge"", ""Owner"": { ""Id"": 22, ""Handle"": ""@amirrajan"" } }");
            };
        }

        void converting_dynamic_model()
        {
            before = () =>
            {
                objectToConvert = new SomeDynamicModel(new { Id = 20, Title = "SomeTitle" });
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["includes both properties from hash and properties defined on dynamic model"] = () =>
            {
                jsonString.should_be(@"{ ""Id"": 20, ""Title"": ""SomeTitle"", ""Name"": ""SomeName"" }");
            };
        }

        [Tag("wip")]
        void converting_named_classes()
        {
            before = () =>
            {
                objectToConvert = new 
                {
                    Goal = new Goal
                    {
                        Cost = 100,
                        Name = "Goal",
                        Expense = new Expense
                        {
                            Amount = 500,
                            Name = "Expense"
                        }
                    }
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["includes serialization of named classes"] = () =>
            {
                jsonString.should_be(@"{ ""Goal"": { ""Name"": ""Goal"", ""Cost"": 100, ""Expense"": { ""Name"": ""Expense"", ""Amount"": 500 } } }");
            };
        }
    }

    public class Goal
    {
        public string Name { get; set; }

        public decimal Cost { get; set; }

        public Expense Expense { get; set; }
    }

    public class Expense
    {
        public string Name { get; set; }

        public decimal Amount { get; set; }
    }
}
