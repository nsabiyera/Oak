using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using System.Data;
using Npgsql;

namespace Oak.Tests.describe_Seed_for_Postgres
{
    class describe_PurgeDb : nspec
    {
        string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Postgres1234;Database=Oak;";

        Seed seed;

        void before_each()
        {
            seed = new Seed(
                new ConnectionProfile
                {
                    ConnectionString = connectionString,
                    ProviderName = "Npgsql"
                });
        }

        void it_can_drop_public_tables()
        {
            seed.ExecuteNonQuery(
                seed.CreateTable(
                    "People",
                    new { Name = "varchar(255)" }
                )
            );

            TableExists("People").should_be(true);

            seed.PurgeDb();

            TableExists("People").should_be(false);
        }

        void it_can_drop_public_table_with_primary_key()
        {
            seed.ExecuteNonQuery(seed.CreateTable(
                "People",
                seed.Id(),
                new { Name = "varchar(255)" }));

            TableExists("People").should_be(true);

            seed.PurgeDb();

            TableExists("People").should_be(false);
        }

        void it_drops_tables_with_foreign_keys()
        {
            seed.ExecuteNonQuery(seed.CreateTable(
                "People",
                seed.Id(),
                new { Name = "varchar(255)" }));

            seed.ExecuteNonQuery(seed.CreateTable(
                "Emails",
                seed.Id(),
                new { EmailId = "int", ForeignKey = "People(Id)" },
                new { Address = "varchar(255)" }));

            TableExists("People").should_be(true);
            TableExists("Emails").should_be(true);

            seed.PurgeDb();
             
            TableExists("People").should_be(false);
            TableExists("Emails").should_be(false);
        }

        bool TableExists(string tableName)
        {
            var query = "select count(*) from Information_Schema.tables where table_name = '{0}'".With(tableName.ToLower());

            var count = Convert.ToInt32(seed.ExecuteScalar(query));

            return count == 1;
        }
    }
}
