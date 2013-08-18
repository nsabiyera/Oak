using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Data;
using Npgsql;

namespace Oak.Tests.describe_DbProviders
{
    class describe_Postgresql : nspec
    {
        string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Postgres1234;Database=Oak;";

        void describe_Seed()
        {
            it["can drop and create tables"] = () => 
            {
                var seed = new Seed(
                    new ConnectionProfile
                    {
                        ConnectionString = connectionString,
                        ProviderName = "Npgsql"
                    });

                seed.ExecuteNonQuery(seed.CreateTable("People", new { Name = "varchar(255)" }));

                seed.ExecuteScalar("select count(*) from Information_Schema.tables where table_name = 'people'").should_be(1);

                seed.ExecuteNonQuery(seed.DropTable("People"));

                seed.ExecuteScalar("select count(*) from Information_Schema.tables where table_name = 'people'").should_be(0);
            };
        }
    }
}
