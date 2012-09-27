using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Microsoft.CSharp.RuntimeBinder;

namespace Oak.Tests
{
    class describe_JsonToDynamic : nspec
    {
        string jsonString;
        dynamic json;

        void converting_json_string_to_dynamic()
        {
            act = () => json = JsonToDynamic.Parse(jsonString);

            context["simple json string: { result: 'hello' }"] = () =>
            {
                before = () => jsonString = "{ result: 'hello' }";

                it["contains property called result with value hello"] = () =>
                    (json.result as string).should_be("hello");
            };

            context["json with two objects: { property1: 'property1', property2: 'property2' }"] = () =>
            {
                before = () => jsonString = "{ property1: 'property1', property2: 'property2' }";

                it["contains property 1"] = () => (json.property1 as string).should_be("property1");

                it["contains property 2"] = () => (json.property2 as string).should_be("property2");
            };

            context["json with a complex object: { result: { FirstName: 'Jane', LastName: 'Doe' } }"] = () =>
            {
                before = () => jsonString = "{ result: { FirstName: 'Jane', LastName: 'Doe' } }";

                it["first name is set to Jane"] = () => (json.result.FirstName as string).should_be("Jane");

                it["last name is set to Doe"] = () => (json.result.LastName as string).should_be("Doe");
            };

            context["json for an array of strings { result: ['item1', 'item2'] }"] = () =>
            {
                before = () => jsonString = "{ result: ['item1', 'item2'] }";

                it["first item is item1"] = () => (json.result[0] as string).should_be("item1");

                it["second item is item2"] = () => (json.result[1] as string).should_be("item2");
            };

            context["json array of complex types { result: [ { FirstName: 'Jane', LastName: 'Doe' }, { FirstName: 'John', LastName: 'Smith' } ] }"] = () =>
            {
                before = () => jsonString = "{ result: [ { FirstName: 'Jane', LastName: 'Doe' }, { FirstName: 'John', LastName: 'Smith' } ] }";

                it["first record's first name is jane"] = () => (json.result[0].FirstName as string).should_be("Jane");

                it["first record's last name is doe"] = () => (json.result[0].LastName as string).should_be("Doe");

                it["last record's first name is john"] = () => (json.result[1].FirstName as string).should_be("John");

                it["last record's last name is smith"] = () => (json.result[1].LastName as string).should_be("Smith");

                it["lambdas can be applied to array"] = () =>
                {
                    var justFirstNames = (json.result as IEnumerable<dynamic>).Select(s => s.FirstName as string);
                    justFirstNames.First().should_be("Jane");
                    justFirstNames.Last().should_be("John");
                };
            };

            context["json array of json arrays { result: [ ['first', 'last'], ['second', 'third'] ] }"] = () =>
            {
                before = () => jsonString = "{ result: [ ['first', 'last'], ['second', 'third'] ] }";

                it["first record's first record is 'first'"] = () => (json.result[0][0] as string).should_be("first");

                it["first record's second record is 'last'"] = () => (json.result[0][1] as string).should_be("last");

                it["second record's first record is 'second'"] = () => (json.result[1][0] as string).should_be("second");

                it["second record's second record is 'third'"] = () => (json.result[1][1] as string).should_be("third");
            };

            context["root object is an array of json objects [ { Name: 'Jane Doe' }, { Name: 'John Doe' } ]"] = () =>
            {
                before = () => jsonString = "[ { Name: 'Jane Doe' }, { Name: 'John Doe' } ]";

                it["first record's name property is Jane Doe"] = () => (json[0].Name as string).should_be("Jane Doe");
            };

            context["root object is a simple string 'Jane Doe'"] = () =>
            {
                before = () => jsonString = "Jane Doe";

                it["returns jane doe"] = () => (json as string).should_be("Jane Doe");
            };

            context["root object is a simple boolean 'true'"] = () =>
            {
                before = () => jsonString = "true";

                it["returns bool"] = () => ((bool)json).should_be(true);
            };

            context["root object is simple integer"] = () =>
            {
                before = () => jsonString = "10";

                it["returns int"] = () => ((int)json).should_be(10);
            };

            context["root object is simple double"] = () =>
            {
                before = () => jsonString = "10.5";

                it["returns double"] = () => ((double)json).should_be((double)10.5);
            };

            context["root object is simple datetime"] = () =>
            {
                before = () => jsonString = "01/01/2010";

                it["returns datetime"] = () => ((DateTime)json).ToString().should_be(DateTime.Parse("01/01/2010").ToString());
            };
        }
    }
}
