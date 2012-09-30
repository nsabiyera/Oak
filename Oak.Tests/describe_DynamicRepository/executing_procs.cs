using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicRepository.Classes;

namespace Oak.Tests.describe_DynamicRepository
{
    class executing_procs : nspec
    {
        Seed seed;

        Records records;

        void before_each()
        {
            seed = new Seed();

            records = new Records();

            seed.PurgeDb();

            @"if exists(select * from sysobjects where name = 'GetRecords' and xtype = 'p')
              begin
	            drop procedure GetRecords	
              end".ExecuteNonQuery();
        }

        void executing_stored_procs()
        {
            before = () =>
            {
                seed.CreateTable("Records", new dynamic[] 
                { 
                   seed.Id(),
                   new { Name = "nvarchar(255)" }
                }).ExecuteNonQuery();

                new { Name = "Name 1" }.InsertInto("Records");

                new { Name = "Name 2" }.InsertInto("Records");

                new { Name = "Other 2" }.InsertInto("Records");

                @"create procedure GetRecords(@filter as nvarchar(255)) as
                  begin
                      select * from Records where name like '%' + @filter + '%'
                  end".ExecuteNonQuery();
            };

            it["proc results are converted to type"] = () =>
            {
                var result = records.Query("GetRecords @0", "Name");

                result.Count().should_be(2);
            };
        }
    }
}
