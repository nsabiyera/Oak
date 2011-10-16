using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Dynamic;

namespace Oak.Tests
{
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
            };

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts expando"] = () =>
                jsonString.should_be(@"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15));
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

            act = () => jsonString = DynamicToJson.Convert(objectToConvert);

            it["converts gemini"] = () =>
                jsonString.should_be(@"{{ ""Id"": {0}, ""String"": ""{1}"", ""Char"": ""{2}"", ""DateTime"": ""{3}"", ""Double"": {4}, ""Guid"": ""{5}"", ""Decimal"": {6} }}"
                    .With(15, "hello", 'a', DateTime.Today, (double)100, Guid.Empty, (decimal)15));
        }

        void describe_dynamic_model_to_json()
        {
            before = () =>
            {
                objectToConvert = new DynamicModel().Init(new
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
                    new DynamicModel().Init(new { Id = 2 }),
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
                { new { Name = "Jane Doe" }, false, "anonymous type" },
                { "Jane Doe", false, "string" },
                { new List<object> { new ExpandoObject(), new Gemini(), new DynamicModel() }, true, "list containing convertable types" },
                { new List<string>() { "Jane", "Doe" }, false, "list of string" },
                { new List<object> { new { Name = "Jane Doe" }, new ExpandoObject(), new Gemini() }, false, "list containing convertable and non-convertable types" },
                { new List<string>(), true, "empty list" }
            }.Do((entity, expectedResult, type) => 
            {
                it["{0} should evaluate to: {1}".With(type, expectedResult)] = () => 
                    DynamicToJson.CanConvertObject(entity as object).should_be(expectedResult);
            });
        }
    }
}
