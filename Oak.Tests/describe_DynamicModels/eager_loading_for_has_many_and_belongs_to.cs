using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests.describe_DynamicModels
{
    class eager_loading_for_has_many_and_belongs_to : _dynamic_models
    {
        object screencastId, screencast2Id, presenterId, presenter2Id, tagId, tag2Id;

        dynamic screencasts;

        void before_each()
        {
            screencasts = new Screencasts();

            seed.PurgeDb();

            seed.CreateTable("Screencasts",
                new { Id = "int" },
                new { Title = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Presenters",
                new { Id = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Tags",
                new { Id = "int" },
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("PresentersScreencasts",
                new { Id = "int" },
                new { PresenterId = "int" },
                new { ScreencastId = "int" }).ExecuteNonQuery();

            seed.CreateTable("ScreencastsTags",
                new { Id = "int" },
                new { ScreencastId = "int" },
                new { TagId = "int" }).ExecuteNonQuery();

            screencastId = 100;

            new { Id = screencastId, Title = "Oak" }.InsertInto("Screencasts");

            screencast2Id = 200;

            new { Id = screencast2Id, Title = "Cambium" }.InsertInto("Screencasts");

            presenterId = 300;

            new { Id = presenterId, Name = "Amir" }.InsertInto("Presenters");

            tagId = 400;
                
            new { Id = tagId, Name = "dynamic" }.InsertInto("Tags");

            tag2Id = 500;
                
            new { Id = tag2Id, Name = "orm" }.InsertInto("Tags");

            new { presenterId, screencastId }.InsertInto("PresentersScreencasts");

            new { screencastId, tagId }.InsertInto("ScreencastsTags");
        }

        void it_loads_and_caches_each_child_collection_specified()
        {
            List<string> sqlQueries = new List<string>();

            DynamicRepository.WriteDevLog = true;

            DynamicRepository.LogSql = new Action<object, string, object[]>(
                (sender, sql, @params) =>
                {
                    sqlQueries.Add(sql);
                });

            var allScreencasts = screencasts.All().Include("Presenters", "Tags");

            ((int)allScreencasts.Count()).should_be(2);

            var firstScreencast = allScreencasts.First();

            ((int)firstScreencast.Presenters().Count()).should_be(1);

            var presenters = firstScreencast.Presenters();

            (presenters.First().Name as string).should_be("Amir");

            ((int)firstScreencast.Tags().Count()).should_be(1);

            var tags = firstScreencast.Tags();

            (tags.First().Name as string).should_be("dynamic");

            var lastScreencast = allScreencasts.Last();

            ((int)lastScreencast.Presenters().Count()).should_be(0);

            sqlQueries.Count.should_be(3);
        }
    }
}
