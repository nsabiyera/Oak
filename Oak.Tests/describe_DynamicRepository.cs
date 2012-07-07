using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Massive;

namespace Oak.Tests
{
    public class Records : DynamicRepository { }

    [Tag("wip")]
    class describe_DynamicRepository : nspec
    {
        Seed seed;

        Records records;

        object recordToInsert;

        static describe_DynamicRepository()
        {
            Gemini.Initialized<Gemini>(d => 
            {
                var hash = d.Hash() as IDictionary<string, object>;

                var assertionsToAdd = new Dictionary<string, object>();

                foreach (var entry in hash)
                {
                    assertionsToAdd.Add(entry.Key + "_is", 
                        new DynamicFunctionWithParam(v => 
                        {
                            (entry.Value as object).should_be(v as object);
                            return null;
                        })
                    );
                }

                foreach (var assertion in assertionsToAdd) d.SetMember(assertion.Key, assertion.Value);
            });
        }

        void before_each()
        {
            seed = new Seed();

            records = new Records();
        }

        void inserting_a_record_that_has_non_value_type_properties()
        {
            before = () =>
            {
                seed.CreateTable("Records", new dynamic[] 
                { 
                   seed.Id(),
                   new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                recordToInsert = new { Name = "foo", NotAValueType = new { Name = "bar" } };
            };

            act = () => records.Insert(recordToInsert);

            it["the record is inserted (ignoring the reference type)"] = () =>
            {
                var record = records.All().First();

                record.Name_is("foo");
            };
        }
    }
}
