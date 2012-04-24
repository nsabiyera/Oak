using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.IO;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class exporting_script : _seed
    {
        void before_each()
        {
            DeleteSqlFiles();
        }

        void it_exports_a_script_that_returns_a_single_script()
        {
            var scripts = new List<Func<string>> 
            {
                new Func<string>(DropTableFoobar)
            };

            seed.Export("", scripts);

            File.Exists("1 - DropTableFoobar.sql").should_be_true();
        }

        void it_exports_a_script_that_returns_multiple_scripts()
        {
            var scripts = new List<dynamic> 
            {
                new Func<dynamic>(DropTables)
            };

            seed.Export("", scripts);

            File.Exists("1 - 1 - DropTables.sql").should_be_true();

            File.Exists("1 - 2 - DropTables.sql").should_be_true();
        }

        string DropTableFoobar()
        {
            return "drop table Foobar";
        }

        IEnumerable<string> DropTables()
        {
            yield return "drop table Foobar1";

            yield return "drop table Foobar2";
        }
    }
}
