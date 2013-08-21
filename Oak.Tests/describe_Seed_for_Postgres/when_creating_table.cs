using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Data;
using Npgsql;

namespace Oak.Tests.describe_Seed_for_Postgres
{
    class when_creating_table : _seed
    {
        void act_each()
        {
            command = seed.CreateTable("Users", columns);
        }

        void generating_a_table_where_column_is_an_identity_column_and_primary_key()
        {
            before = () =>
                columns = new dynamic[]
                {
                    new { Id = "SERIAL", Identity = true, PrimaryKey = true },
                };

            it["contains identity definition"] = () =>
                CommandShouldBe(@"
                    CREATE TABLE public.Users
                    (                                                 
                      Id SERIAL NOT NULL, 
                      CONSTRAINT PK_Users PRIMARY KEY 
                      (
                        Id
                      )                                             
                    )
                ");
        }
    }
}
