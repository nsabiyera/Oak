using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Text;
using NSpec;
using System.Data;
using Npgsql;

namespace Oak.Tests.describe_Seed_for_Postgres
{
    class _seed_for_postgres : nspec
    {
        protected Seed seed;
        protected string command;
        protected dynamic[] columns;
        ConnectionProfile connectionProfile;
        string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password=Postgres1234;Database=Oak;";

        void before_each()
        {
            connectionProfile = new ConnectionProfile
            {
                ConnectionString = connectionString,
                ProviderName = "Npgsql"
            };

            seed = new Seed(connectionProfile);
        }


        public void CommandShouldBe(string expected)
        {
            command.should_be(ToSingleLine(expected));
        }

        public string ToSingleLine(string s)
        {
            var single = Regex.Replace(s, @"[ ]{2,}", "");

            single = single.Trim().Replace(Environment.NewLine, "");

            return single;
        }
    }
}
