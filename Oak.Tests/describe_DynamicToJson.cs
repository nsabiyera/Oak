using System;
using System.Collections.Generic;
using System.Linq;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests
{
    [Tag("wip")]
    class describe_DynamicToJson : nspec
    {
        dynamic objectToConvert;

        string jsonString;

        void before_each()
        {
            jsonString = null;

            tasks = new Tasks();
        }

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
                string expected = @"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
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
                    string expected = @"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
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
                    string expected = @"[ { ""Id"": 1 }, { ""Id"": 2 }, { ""Id"": 3 } ]";

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
                    string expected = @"{{ ""FirstName"": ""{0}"", ""LastName"": ""{1}"" }}".With("Jane", "Doe");

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
                    string expected = @"{ ""Users"": [ { ""Name"": ""Jane"" }, { ""Name"": ""Jake"" } ] }";

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
                    string expected = @"{ ""Users"": [ ""Jane"", ""Doe"" ] }";

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
                    string expected = @"{ ""Users"": [ 10, 20 ] }";

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
                string expected = @"{ ""IsAdded"": true, ""Users"": [ true, false ] }";

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
                    string expected = @"{ ""Id"": 15, ""Name"": ""Mirror's Edge"", ""Owner"": { ""Id"": 22, ""Handle"": ""@amirrajan"" } }";

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
                string expected = @"{ ""Id"": 20, ""Title"": ""SomeTitle"", ""Name"": ""SomeName"" }";

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
                string expected = @"{ ""Goal"": { ""Name"": ""Goal"", ""Cost"": 100, ""Expense"": { ""Name"": ""Expense"", ""Amount"": 500 } } }";

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
                string expected = @"{ ""Quotes"": ""\""Quoted\"""", ""Ticks"": ""'Ticked'"", ""BackSlashes"": ""c:\\Temp"", ""NewLine"": ""New\r\nLine"" }";

                jsonString.should_be(expected);
            };
        }

        dynamic tasks;

        void describe_db_rows_to_json()
        {
            before = () =>
            {
                Seed seed = new Seed();

                seed.PurgeDb();

                seed.CreateTable("Rabbits", seed.Id(), new { Name = "nvarchar(255)" }).ExecuteNonQuery();

                seed.CreateTable("Tasks",
                    seed.Id(),
                    new { Description = "nvarchar(255)" },
                    new { RabbitId = "int" },
                    new { DueDate = "datetime" }).ExecuteNonQuery();

                var rabbitId = new { Name = "Yours Truly" }.InsertInto("Rabbits");

                new { rabbitId, Description = "bolt onto vans", DueDate = DateTime.Today }.InsertInto("Tasks");

                rabbitId = new { Name = "Hiro Protaganist" }.InsertInto("Rabbits");

                new { rabbitId, Description = "save the world", DueDate = DateTime.Today }.InsertInto("Tasks");

                new { rabbitId, Description = "deliver pizza", DueDate = DateTime.Today }.InsertInto("Tasks");

                rabbitId = new { Name = "Lots" }.InsertInto("Rabbits");

                for (int i = 0; i < 10; i++)
                {
                    new
                    {
                        rabbitId,
                        Description = "Task: " + i.ToString(),
                        DueDate = DateTime.Today
                    }.InsertInto("Tasks");
                }
            };

            it["disregards self referencing objects"] = () =>
            {
                var results = tasks.All().Include("Rabbits").ToList();

                (results as IEnumerable<dynamic>).ForEach(s =>
                {
                    s.Rabbit = s.Rabbit();
                });

                objectToConvert = new Gemini(new { Tasks = results });
                string expected = @"{ ""Tasks"": [ { ""Id"": 1, ""Description"": ""bolt onto vans"", ""RabbitId"": 1, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 1, ""Name"": ""Yours Truly"" } }, { ""Id"": 2, ""Description"": ""save the world"", ""RabbitId"": 2, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 2, ""Name"": ""Hiro Protaganist"" } }, { ""Id"": 3, ""Description"": ""deliver pizza"", ""RabbitId"": 2, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 2, ""Name"": ""Hiro Protaganist"" } }, { ""Id"": 4, ""Description"": ""Task: 0"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 5, ""Description"": ""Task: 1"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 6, ""Description"": ""Task: 2"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 7, ""Description"": ""Task: 3"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 8, ""Description"": ""Task: 4"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 9, ""Description"": ""Task: 5"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 10, ""Description"": ""Task: 6"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 11, ""Description"": ""Task: 7"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 12, ""Description"": ""Task: 8"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } }, { ""Id"": 13, ""Description"": ""Task: 9"", ""RabbitId"": 3, ""DueDate"": ""1/14/2013 12:00:00 AM"", ""Rabbit"": { ""Id"": 3, ""Name"": ""Lots"" } } ] }";
                jsonString = DynamicToJson.Convert(objectToConvert);
                jsonString.should_be(expected);

            };
        }

        void describe_collections_that_have_collections()
        {
            before = () =>
            {
                dynamic person1 = new Gemini(new { Name = "Jane Doe", Friends = new List<string> { "A", "B", "C" } });

                objectToConvert = new List<dynamic> { person1 };
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["serializes the list for both"] = () =>
            {
                var expected = @"[ { ""Name"": ""Jane Doe"", ""Friends"": [ ""A"", ""B"", ""C"" ] } ]";

                jsonString.should_be(expected);
            };
        }

        void describe_two_objects_referencing_the_same_list()
        {
            before = () =>
            {
                dynamic person1 = new Gemini(new { Name = "Jane Doe" });

                dynamic person2 = new Gemini(new { Name = "John Doe" });

                dynamic person3 = new Gemini(new { Name = "Jane Smith" });

                var friends = new List<dynamic>
                {
                    new Gemini(new { Name = "John Smith", Friends = new List<dynamic> { person1, person2, person3 } })
                };

                person1.Friends = friends;

                person2.Friends = friends;

                objectToConvert = new Gemini(new
                {
                    People = new List<dynamic>() 
                    {
                        person1,
                        person2
                    }
                });
            };

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert);
            };

            it["serializes the list for both"] = () =>
            {
                var expected = @"{ ""People"": [ { ""Name"": ""Jane Doe"", ""Friends"": [ { ""Name"": ""John Smith"", ""Friends"": [ { ""Name"": ""Jane Smith"" } ] } ] }, { ""Name"": ""John Doe"", ""Friends"": [ { ""Name"": ""John Smith"", ""Friends"": [ { ""Name"": ""Jane Smith"" } ] } ] } ] }";

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
