using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class renaming_column : _seed
    {
        void it_works()
        {
            seed.PurgeDb();

            seed.CreateTable("Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            "insert into Table1 values('Foobar');".ExecuteNonQuery();

            "select top 1 Name from Table1".ExecuteScalar().should_be("Foobar");

            seed.RenameColumn("Table1", "Name", "NewName").ExecuteNonQuery();

            "select top 1 NewName from Table1".ExecuteScalar().should_be("Foobar");
        }
    }
}
