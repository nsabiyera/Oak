using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;
using Massive;

namespace Oak.Tests.describe_DynamicRepository
{
    class detecting_inefficient_queries : nspec
    {
        void specify_exact_matches_on_the_same_thread_and_stack_trace_are_considered_inefficient()
        {
            var sqlLog = new SqlQueryLog(this,
                "select * from User where email = 'user@example.com'",
                "stack trace",
                50,
                null);

            var sqlLog2 = new SqlQueryLog(this,
                "select * from User where email = 'user@example.com'",
                "stack trace",
                50,
                null);

            var inefficientQueries = Bullet.InefficientQueries(new [] { sqlLog, sqlLog2 }.ToList());

            inefficientQueries.Count().should_be(2);

            (inefficientQueries.First().Reason as string).should_contain("redundant");
        }

        void specify_similiar_queries_are_considered_nPlus1_if_the_where_in_clause_is_different_but_the_stack_and_thread_are_the_same()
        {
            var sqlLog = new SqlQueryLog(this,
                "select * from User where email in ('user@example.com')",
                "stack trace",
                50,
                null);

            var sqlLog2 = new SqlQueryLog(this,
                "select * from User where email in ('user2@example.com')",
                "stack trace",
                50,
                null);

            var inefficientQueries = Bullet.InefficientQueries(new [] { sqlLog, sqlLog2 }.ToList());

            inefficientQueries.Count().should_be(2);

            (inefficientQueries.First().Reason as string).should_contain("N+1");
        }

        void specify_queries_on_the_same_thread_with_different_stacktraces_are_worth_looking_at_but_my_not_be_inefficient()
        {
            var sqlLog = new SqlQueryLog(this,
                "select * from User where email in ('user@example.com')",
                "stack trace",
                50,
                null);

            var sqlLog2 = new SqlQueryLog(this,
                "select * from User where email in ('user2@example.com')",
                "stack trace 2",
                50,
                null);

            var inefficientQueries = Bullet.InefficientQueries(new [] { sqlLog, sqlLog2 }.ToList());

            inefficientQueries.Count().should_be(2);

            (inefficientQueries.First().Reason as string).should_contain("may not be inefficient");
        }
    }
}
