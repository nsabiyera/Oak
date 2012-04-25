using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class executing_scripts : _seed
    {
        void it_executes_scripts_up_to_specified_script()
        {
            seed.PurgeDb();

            seed.ExecuteUpTo(Scripts(), Script2);

            TableExists("Table1").should_be_true();

            TableExists("Table2").should_be_false();

            TableExists("Table3").should_be_false();
        }

        void it_executes_scripts_thru_specified_script()
        {
            seed.PurgeDb();

            seed.ExecuteTo(Scripts(), Script2);

            TableExists("Table1").should_be_true();

            TableExists("Table2").should_be_true();

            TableExists("Table3").should_be_false();
        }

        IEnumerable<Func<dynamic>> Scripts()
        {
            yield return Script1;

            yield return Script2;

            yield return Script3;
        }

        string Script1()
        {
            return seed.CreateTable("Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            });
        }

        string Script2()
        {
            return seed.CreateTable("Table2", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            });
        }

        string Script3()
        {
            return seed.CreateTable("Table3", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            });
        }
    }
}
