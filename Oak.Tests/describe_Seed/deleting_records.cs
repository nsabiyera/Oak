using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_Seed
{
    class deleting_records : _seed
    {
        void for_sql()
        {
            if(!IsSql()) return;

            it["deletes all records in all tables"] = () => 
            {
                DeleteAllRecords();
                
                RowCount("Table1").should_be(0);

                RowCount("Table2").should_be(0);
            };

            it["enables key constraints after deletion"] = () => 
            {
                DeleteAllRecords();

                var exceptionThrownOnInsert = false;

                try { "insert into Table2 values(1, 'Name2')".ExecuteNonQuery(); }

                catch { exceptionThrownOnInsert = true; }
                    
                exceptionThrownOnInsert.should_be_true();
            };
        }

        void for_sql_ce()
        {
            if(!IsSqlCe()) return;

            it["isn't support"] = () => 
            {
                NotSupportedException actualEx = null;

                try { seed.DeleteAllRecords(); }

                catch(NotSupportedException ex) { actualEx = ex; }

                actualEx.Message.should_contain("Use PurgeDb");
            };
        }

        void DeleteAllRecords()
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


            var table1Id = @"
            insert into Table1(name) values('Name1')
            GO
            select @@identity".ExecuteScalar();

            "insert into Table2(table1id, name) values({0}, 'Name2')".With(table1Id).ExecuteNonQuery();

            RowCount("Table1").should_be(1);

            RowCount("Table2").should_be(1);

            seed.DeleteAllRecords();
        }

        int RowCount(string table)
        {
            return (int)"select count(*) from {0}".With(table).ExecuteScalar();
        }
    }
}
