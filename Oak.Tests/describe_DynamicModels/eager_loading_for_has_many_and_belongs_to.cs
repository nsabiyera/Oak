using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    public class Screencasts : DynamicRepository
    {
        public Screencasts()
        {
            Projection = d => new Screencast(d);
        }
    }

    public class Presenters : DynamicRepository { }

    public class Tags : DynamicRepository { }

    public class Screencast : DynamicModel
    {
        Presenters presenters = new Presenters();

        Screencasts screencasts = new Screencasts();

        Tags tags = new Tags();

        public Screencast(object dto) : base(dto) { }

        public Screencast() { }

        IEnumerable<dynamic> Associates()
        {
            yield return new HasManyAndBelongsTo(presenters, screencasts);

            yield return new HasManyAndBelongsTo(tags, screencasts);
        }
    }

    class eager_loading_for_has_many_and_belongs_to : _dynamic_models
    {
        object screencastId, screencast2Id, presenterId, presenter2Id, tagId, tag2Id;

        dynamic screencasts;

        void before_each()
        {
            screencasts = new Screencasts();

            seed.PurgeDb();

            seed.CreateTable("Screencasts",
                seed.Id(),
                new { Title = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Presenters",
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("Tags",
                seed.Id(),
                new { Name = "nvarchar(255)" }).ExecuteNonQuery();

            seed.CreateTable("PresentersScreencasts",
                seed.Id(),
                new { PresenterId = "int" },
                new { ScreencastId = "int" }).ExecuteNonQuery();

            seed.CreateTable("ScreencastsTags",
                seed.Id(),
                new { ScreencastId = "int" },
                new { TagId = "int" }).ExecuteNonQuery();

            screencastId = new { Title = "Oak" }.InsertInto("Screencasts");

            screencast2Id = new { Title = "Cambium" }.InsertInto("Screencasts");

            presenterId = new { Name = "Amir" }.InsertInto("Presenters");

            presenter2Id = new { Name = "Another" }.InsertInto("Presenters");

            tagId = new { Name = "dynamic" }.InsertInto("Tags");

            tag2Id = new { Name = "orm" }.InsertInto("Tags");

            new { presenterId, screencastId }.InsertInto("PresentersScreencasts");

            new { screencastId, tagId }.InsertInto("ScreencastsTags");
        }

        void it_loads_and_caches_each_child_collection_specified()
        {
            var allScreencasts = screencasts.All().Include("Presenters", "Tags");

            ((int)allScreencasts.Count()).should_be(2);

            new { presenterId = presenter2Id, screencastId }.InsertInto("PresentersScreencasts");

            new { screencastId, tagId = tag2Id }.InsertInto("ScreencastsTags");

            var firstScreencast = allScreencasts.First();

            ((int)firstScreencast.Presenters().Count()).should_be(1);

            ((int)firstScreencast.Tags().Count()).should_be(1);

            var lastScreencast = allScreencasts.Last();

            new { presenterId = presenter2Id, screencastId = screencast2Id }.InsertInto("PresentersScreencasts");

            ((int)lastScreencast.Presenters().Count()).should_be(0);
        }
    }
}
