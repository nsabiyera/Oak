using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;
using Newtonsoft.Json;
using Massive;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests
{
    class describe_DynamicToJson : nspec
    {
        dynamic objectToConvert;

        string jsonString;

        void before_each()
        {
            jsonString = null;

            tasks = new Tasks();
        }

        dynamic tasks;

        [Tag("wip")]
        void describe_db_rows_to_json()
        {
            before = () =>
            {
                Seed seed = new Seed();

                seed.PurgeDb();

                seed.CreateTable("Rabbits", seed.Id(), new { Name = "nvarchar(255)" }).ExecuteNonQuery();

                seed.CreateTable("Tasks", seed.Id(), new { Description = "nvarchar(255)" }, new { RabbitId = "int" }).ExecuteNonQuery();

                var rabbitId = new { Name = "YT" }.InsertInto("Rabbits");

                new { Description = "bolt onto vans", rabbitId }.InsertInto("Tasks");

                new { Description = "save the world", rabbitId }.InsertInto("Tasks");
            };

            it["disregards self referencing objects"] = () =>
            {
                var results = tasks.All().Include("Rabbits").ToList();

                (results as IEnumerable<dynamic>).ForEach(s =>
                {
                    s.Rabbit = s.Rabbit();
                });

                dynamic newGemini = new Gemini(new { Tasks = results });

                //System.Diagnostics.Debugger.Launch();

                string jsonString = DynamicToJson.Convert(newGemini);

                string expected = @"{ ""Tasks"": [ { ""Id"": 1, ""Description"": ""bolt onto vans"", ""RabbitId"": 1, ""Rabbit"": { ""Id"": 1, ""Name"": ""YT"" } }, { ""Id"": 2, ""Description"": ""save the world"", ""RabbitId"": 1, ""Rabbit"": { ""Id"": 1, ""Name"": ""YT"" } } ] }";

                var session = new DeserializationSession();

                //System.Diagnostics.Debugger.Launch();
                r = new Item(newGemini, session).V;
                //jsonString.should_be(expected);
                r.should_be(expected);
            };
        }

        private string r;

        [Tag("wip")]
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
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);

            };

            it["converts prototype"] = () =>
            {
                var expected = @"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6}, ""StringAsNull"": {7}, ""Long"": 100 }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15, "null");
                //System.Diagnostics.Debugger.Launch();
                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
            };
        }

        [Tag("wip")]
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
                string expected = @"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15);

                //System.Diagnostics.Debugger.Launch();
                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
            };
        }

        [Tag("wip")]
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
                    string expected = @"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                        .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15);

                    //System.Diagnostics.Debugger.Launch();
                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
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
                    string expected = @"[ { ""Id"": 1 }, { ""Id"": 2 }, { ""Id"": 3 } ]";

                    //System.Diagnostics.Debugger.Launch();
                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
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

        [Tag("wip")]
        void describe_anonymous_type_to_json()
        {
            before = () => objectToConvert = new { FirstName = "Jane", LastName = "Doe" };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts properties of anonymous type"] = () =>
                {
                    string expected = @"{{ ""FirstName"": ""{0}"", ""LastName"": ""{1}"" }}".With("Jane", "Doe");

                    //System.Diagnostics.Debugger.Launch();
                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
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
                    //System.Diagnostics.Debugger.Launch();
                    jsonString = DynamicToJson.Convert(objectToConvert);
                };

            it["executes deferred statement and serializes result"] = () =>
                {
                    string expected = @"{ ""Users"": [ { ""Name"": ""Jane"" }, { ""Name"": ""Jake"" } ] }";


                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
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
                    string expected = @"{ ""Users"": [ ""Jane"", ""Doe"" ] }";

                    //System.Diagnostics.Debugger.Launch();
                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
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
                    string expected = @"{ ""Users"": [ 10, 20 ] }";

                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
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
                string expected = @"{ ""IsAdded"": true, ""Users"": [ true, false ] }";

                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
            };
        }

        [Tag("wip")]
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
                    string expected = @"{ ""Id"": 15, ""Name"": ""Mirror's Edge"", ""Owner"": { ""Id"": 22, ""Handle"": ""@amirrajan"" } }";

                    var session = new DeserializationSession();
                    r = new Item(objectToConvert, session).V;
                    jsonString.should_be(expected);
                    r.should_be(expected);
                };
        }

        [Tag("wip")]
        void converting_dynamic_model()
        {
            before = () =>
            {
                objectToConvert = new SomeDynamicModel(new { Id = 20, Title = "SomeTitle" });
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["includes both properties from hash and properties defined on dynamic model"] = () =>
            {
                string expected = @"{ ""Id"": 20, ""Title"": ""SomeTitle"", ""Name"": ""SomeName"" }";

                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
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
                string expected = @"{ ""Goal"": { ""Name"": ""Goal"", ""Cost"": 100, ""Expense"": { ""Name"": ""Expense"", ""Amount"": 500 } } }";

                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
            };
        }

        [Tag("wip")]
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
                //System.Diagnostics.Debugger.Launch();

                string expected = @"{ ""Quotes"": ""\""Quoted\"""", ""Ticks"": ""'Ticked'"", ""BackSlashes"": ""c:\\Temp"", ""NewLine"": ""New\r\nLine"" }";

                var session = new DeserializationSession();
                r = new Item(objectToConvert, session).V;
                jsonString.should_be(expected);
                r.should_be(expected);
            };
        }
    }

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
}
