using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class execute_non_query : _seed
    {
        void before_each()
        {
            seed.PurgeDb();
        }

        void specify_a_single_string_query_is_executed_by_seed()
        {
            seed.ExecuteNonQuery(seed.CreateTable("SingleQueryTable", new dynamic[] 
            {  
                seed.Id()
            }));

            TableExists("SingleQueryTable").should_be_true();
        }

        void specify_a_function_that_returns_a_string_is_executed_by_seed()
        {
            var script = new Func<string>(() =>
                seed.CreateTable("SingleQueryTable", new dynamic[] 
                {  
                    seed.Id()
                }));

            seed.ExecuteNonQuery(script);

            TableExists("SingleQueryTable").should_be_true();
        }

        void specify_a_function_that_returns_a_collection_is_executed_by_seed()
        {
            var script = new Func<IEnumerable<string>>(() =>
            {
                var list = new List<string>();

                list.Add(seed.CreateTable("SingleQueryTable", new dynamic[] 
                {  
                    seed.Id()
                }));

                list.Add(seed.CreateTable("ListQueryTable", new dynamic[] 
                {  
                    seed.Id()
                }));

                return list;
            });

            seed.ExecuteNonQuery(script);

            TableExists("SingleQueryTable").should_be_true();

            TableExists("ListQueryTable").should_be_true();
        }
    }
}
