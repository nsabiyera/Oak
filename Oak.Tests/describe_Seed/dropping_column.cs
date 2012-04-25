using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class dropping_column : _seed
    {
        void it_works()
        {
            seed.PurgeDb();

            seed.CreateTable("Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.DropColumn("Table1", "Name").ExecuteNonQuery();

            Columns("Table1").ToList().should_not_contain("Name");
        }

        void specify_constraints_can_be_dropped_too()
        {
            seed.PurgeDb();

            seed.CreateTable("Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.DropConstraint("Table1", "Id").ExecuteNonQuery();

            seed.DropColumn("Table1", "Id").ExecuteNonQuery();
        }
    }
}
