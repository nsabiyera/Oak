using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicToJson
{
    class casing : _describe_DynamicToJson
    {
        void describe_casing()
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

            act = () =>
            {
                jsonString = DynamicToJson.Convert(objectToConvert, new NoCasingChange());
            };

            it["serializes with altered casing"] = () =>
            {
                var expected = @"{ ""Id"": 15, ""Name"": ""Mirror's Edge"", ""Owner"": { ""Id"": 22, ""Handle"": ""@amirrajan"" } }";

                jsonString.should_be(expected);
            };
        }
    }
}
