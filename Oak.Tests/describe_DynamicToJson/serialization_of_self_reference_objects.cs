using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak.Tests.describe_DynamicModels.Classes;
using NSpec;

namespace Oak.Tests.describe_DynamicToJson
{
    class serialization_of_self_reference_objects : _describe_DynamicToJson
    {
        dynamic tasks;

        void before_each()
        {
            tasks = new Tasks();
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
                var expected = @"[ { ""name"": ""Jane Doe"", ""friends"": [ ""A"", ""B"", ""C"" ] } ]";

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
                var expected = @"{ ""people"": [ { ""name"": ""Jane Doe"", ""friends"": [ { ""name"": ""John Smith"", ""friends"": [ { ""name"": ""Jane Smith"" } ] } ] }, { ""name"": ""John Doe"", ""friends"": [ { ""name"": ""John Smith"", ""friends"": [ { ""name"": ""Jane Smith"" } ] } ] } ] }";

                jsonString.should_be(expected);
            };
        }

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

                new { rabbitId, Description = "bolt onto vans", DueDate = new DateTime(2013, 1, 14) }.InsertInto("Tasks");

                rabbitId = new { Name = "Hiro Protaganist" }.InsertInto("Rabbits");

                new { rabbitId, Description = "save the world", DueDate = new DateTime(2013, 1, 14) }.InsertInto("Tasks");

                new { rabbitId, Description = "deliver pizza", DueDate = new DateTime(2013, 1, 14) }.InsertInto("Tasks");

                rabbitId = new { Name = "Lots" }.InsertInto("Rabbits");

                for (int i = 0; i < 10; i++)
                {
                    new
                    {
                        rabbitId,
                        Description = "Task: " + i.ToString(),
                        DueDate = new DateTime(2013, 1, 14)
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
                string expected = @"{ ""tasks"": [ { ""id"": 1, ""description"": ""bolt onto vans"", ""rabbitId"": 1, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 1, ""name"": ""Yours Truly"" } }, { ""id"": 2, ""description"": ""save the world"", ""rabbitId"": 2, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 2, ""name"": ""Hiro Protaganist"" } }, { ""id"": 3, ""description"": ""deliver pizza"", ""rabbitId"": 2, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 2, ""name"": ""Hiro Protaganist"" } }, { ""id"": 4, ""description"": ""Task: 0"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 5, ""description"": ""Task: 1"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 6, ""description"": ""Task: 2"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 7, ""description"": ""Task: 3"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 8, ""description"": ""Task: 4"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 9, ""description"": ""Task: 5"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 10, ""description"": ""Task: 6"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 11, ""description"": ""Task: 7"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 12, ""description"": ""Task: 8"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } }, { ""id"": 13, ""description"": ""Task: 9"", ""rabbitId"": 3, ""dueDate"": ""1/14/2013 12:00:00 AM"", ""rabbit"": { ""id"": 3, ""name"": ""Lots"" } } ] }";
                jsonString = DynamicToJson.Convert(objectToConvert);
                jsonString.should_be(expected);
            };
        }
    }
}
