using System;
using System.Collections.Generic;
using System.Linq;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicToJson
{
    public class SomeDynamicModel : DynamicModel
    {
        string voodoo;
        public SomeDynamicModel(object dto)
            : base(dto)
        {
        }

        public string Name
        {
            get { return "SomeName"; }
        }

        public string Voodoo
        {
            set { voodoo = value; }
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

    class simple_serializations : _describe_DynamicToJson
    {
        void describe_prototype_to_json()
        {
            before = () =>
            {
                objectToConvert = new Prototype();
                objectToConvert.Id = 15;
                objectToConvert.String = "hello";
                objectToConvert.Char = 'a';
                objectToConvert.DateTime = DateTime.Today;
                objectToConvert.Double = (double)100;
                objectToConvert.Guid = Guid.Empty;
                objectToConvert.Decimal = (decimal)15;
                objectToConvert.StringAsNull = null as string;
                objectToConvert.Long = (long)100;
                objectToConvert.S = "property with single character";
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["converts prototype"] = () =>
            {
                var expected = @"{{ ""id"": {0}, ""string"": ""{1}"", ""char"": ""{2}"", ""dateTime"": ""{3}"", ""double"": {4}, ""guid"": ""{5}"", ""decimal"": {6}, ""stringAsNull"": {7}, ""long"": 100, ""s"": ""property with single character"" }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15, "null");

                jsonString.should_be(expected);
            };
        }

        void describe_gemini_to_json()
        {
            before = () =>
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
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["converts gemini"] = () =>
            {
                string expected = @"{{ ""id"": {0}, ""string"": ""{1}"", ""char"": ""{2}"", ""dateTime"": ""{3}"", ""double"": {4}, ""guid"": ""{5}"", ""decimal"": {6} }}"
                                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15);

                jsonString.should_be(expected);
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
            {
                string expected = @"{{ ""id"": {0}, ""string"": ""{1}"", ""char"": ""{2}"", ""dateTime"": ""{3}"", ""double"": {4}, ""guid"": ""{5}"", ""decimal"": {6} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15);

                jsonString.should_be(expected);
            };
        }

        void describe_collection()
        {
            before = () =>
            {
                dynamic prototype = new Prototype();
                prototype.Id = 1;

                objectToConvert = new List<dynamic>
                {
                    prototype,
                    new DynamicModel(new { Id = 2 }),
                    new Gemini(new { Id = 3 }),
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts collection"] = () =>
            {
                string expected = @"[ { ""id"": 1 }, { ""id"": 2 }, { ""id"": 3 } ]";

                jsonString.should_be(expected);
            };
        }

        void describe_can_be_converted()
        {
            new Each<dynamic, bool, string>()
            {
                { new Prototype(), true, "prototype" },
                { new Gemini(), true, "gemini" },
                { new DynamicModel(), true, "dynamic model" },
                { new { Name = "Jane Doe" }, true, "anonymous type" },
                { "Jane Doe", false, "string" },
                { new List<object> { new Prototype(), new Gemini(), new DynamicModel() }, true, "list containing convertable types" },
                { new List<string>() { "Jane", "Doe" }, false, "list of string" },
                { new List<object> { new { Name = "Jane Doe" }, new Prototype(), new Gemini() }, true, "list containing gemini's and anonymous types" },
                { new List<object> { new { Name = "Jane Doe" }, "Foobar" }, false, "list containing convertable types and non convertable types" },
                { new List<string>(), true, "empty list" }
            }.Do((entity, expectedResult, type) =>
            {
                it["{0} should evaluate to: {1}".With(type, expectedResult)] = () =>
                {
                    DynamicToJson.CanConvertObject(entity as object).should_be(expectedResult);
                };
            });
        }

        void describe_anonymous_type_to_json()
        {
            before = () => objectToConvert = new { FirstName = "Jane", LastName = "Doe" };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts properties of anonymous type"] = () =>
            {
                string expected = @"{{ ""firstName"": ""{0}"", ""lastName"": ""{1}"" }}".With("Jane", "Doe");

                jsonString.should_be(expected);
            };
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

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["executes deferred statement and serializes result"] = () =>
            {
                string expected = @"{ ""users"": [ { ""name"": ""Jane"" }, { ""name"": ""Jake"" } ] }";

                jsonString.should_be(expected);
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
                string expected = @"{ ""users"": [ ""Jane"", ""Doe"" ] }";

                jsonString.should_be(expected);
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
                string expected = @"{ ""users"": [ 10, 20 ] }";

                jsonString.should_be(expected);
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
                string expected = @"{ ""isAdded"": true, ""users"": [ true, false ] }";

                jsonString.should_be(expected);
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
                string expected = @"{ ""id"": 15, ""name"": ""Mirror's Edge"", ""owner"": { ""id"": 22, ""handle"": ""@amirrajan"" } }";

                jsonString.should_be(expected);
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
                string expected = @"{ ""id"": 20, ""title"": ""SomeTitle"", ""name"": ""SomeName"" }";

                jsonString.should_be(expected);
            };
        }

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
                string expected = @"{ ""goal"": { ""name"": ""Goal"", ""cost"": 100, ""expense"": { ""name"": ""Expense"", ""amount"": 500 } } }";

                jsonString.should_be(expected);
            };
        }

        void escaping_strings()
        {
            before = () =>
            {
                objectToConvert = new
                {
                    Quotes = @"""Quoted""",
                    Ticks = @"'Ticked'", //ticks don't need to be escaped, jquery for some reason refuses to deserialize a payload if ticks are escaped when they dont need to be
                    BackSlashes = @"c:\Temp",
                    NewLine = "New" + Environment.NewLine + "Line"
                };
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["special characters are escaped"] = () =>
            {
                string expected = @"{ ""quotes"": ""\""Quoted\"""", ""ticks"": ""'Ticked'"", ""backSlashes"": ""c:\\Temp"", ""newLine"": ""New\r\nLine"" }";

                jsonString.should_be(expected);
            };
        }

        void describe_key_value_pair_serialization()
        {
            before = () =>
            {
                dynamic rabbit = new Rabbit(new { });

                rabbit.IsValid();

                objectToConvert = new { Errors = rabbit.Errors() };
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["serializes key value pairs (structs)"] = () =>
            {
                var expected = @"{ ""errors"": [ { ""key"": ""Name"", ""value"": ""Name is required."" } ] }";

                jsonString.should_be(expected);
            };
        }

        void describe_empty_collection()
        {
            before = () =>
            {
                objectToConvert = new List<dynamic>();
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["serializes empty list"] = () =>
            {
                var expected = @"[  ]";

                jsonString.should_be(expected);
            };
        }
    }
}
