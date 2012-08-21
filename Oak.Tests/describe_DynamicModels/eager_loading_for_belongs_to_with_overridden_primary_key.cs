using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_belongs_to_with_overridden_primary_key : nspec
    {
        dynamic agents;

        Seed seed;

        void before_each()
        {
            seed = new Seed();

            seed.PurgeDb();

            agents = new Agents();

            seed.CreateTable("Booths",
                seed.Id(),
                new { BoothId = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Agents",
                new { Id = "int" },
                new { BoothId = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            new { BoothId = 500, Name = "Booth 500" }.InsertInto("Booths");

            new { BoothId = 500, Name = "Agent 1" }.InsertInto("Agents");
        }

        void it_retrieve_records_respecting_primary_key()
        {
            var agentResults = agents.All().Include("Booth");

            (agentResults.First().Booth().Name as string).should_be("Booth 500");
        }
    }
}
