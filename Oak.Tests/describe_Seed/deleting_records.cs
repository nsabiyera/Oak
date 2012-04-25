using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    [Tag("wip")]
    class deleting_records : _seed
    {
        void act_each()
        {
            seed.PurgeDb();

            seed.CreateTable("Table1", new dynamic[] 
            { 
                seed.Id(),
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            seed.CreateTable("Table2", new dynamic[] 
            { 
                seed.Id(),
                new { Table1Id = "int", ForeignKey = "Table1(Id)" },
                new { Name = "nvarchar(255)" }
            }).ExecuteNonQuery();

            var table1Id = "insert into Table1 values('Name1'); select scope_identity()".ExecuteScalar();

            "insert into Table2 values({0}, 'Name2')".With(table1Id).ExecuteNonQuery();

            RowCount("Table1").should_be(1);

            RowCount("Table2").should_be(1);

            seed.DeleteAllRecords();
        }

        void it_deletes_all_records_in_all_tables_in_the_database()
        {
            RowCount("Table1").should_be(0);

            RowCount("Table2").should_be(0);
        }

        void it_key_contraints_are_reenabled_after_deletion()
        {
            var exceptionThrownOnInsert = false;

            try { "insert into Table2 values(1, 'Name2')".ExecuteNonQuery(); }

            catch { exceptionThrownOnInsert = true; }
                
            exceptionThrownOnInsert.should_be_true();
        }

        int RowCount(string table)
        {
            return (int)"select count(*) from {0}".With(table).ExecuteScalar();
        }
    }
}
