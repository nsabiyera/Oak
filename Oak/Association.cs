using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace Oak
{
    public class MixInAssociation
    {
        public MixInAssociation(DynamicModel mixWith)
        {
            if(mixWith.GetType().GetMethod("Associates") != null)
            {
                IEnumerable<dynamic> associations = (mixWith as dynamic).Associates();

                foreach (dynamic association in associations)
                {
                    association.Init(mixWith);
                }
            }
        }
    }

    public class Association
    {
        public virtual void Init(dynamic model)
        {

        }

        public string MakeSingular(object o)
        {
            return o.GetType().Name.Substring(0, o.GetType().Name.Length - 1);
        }

        public string SigularizedIdFor(object o)
        {
            return MakeSingular(o) + "Id";
        }
    }

    public class HasMany : Association
    {
        string named;

        DynamicRepository repository;

        public DynamicRepository Through { get; set; }

        public string Using { get; set; }

        public HasMany(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public HasMany(DynamicRepository repository, string named)
        {
            this.repository = repository;

            this.named = named ?? repository.GetType().Name;
        }

        public override void Init(dynamic model)
        {
            var fromColumn = model.GetType().Name + "Id";

            var toTable = repository.GetType().Name;

            if (Through == null)
            {
                (model.Virtual as Prototype).SetMember(
                    named,
                    DirectTableQuery(fromColumn, model));
            }
            else
            {
                (model.Virtual as Prototype).SetMember(
                    named,
                    ThroughTableQuery(fromColumn, toTable, Through.GetType().Name, Using ?? SigularizedIdFor(repository), model));
            }
        }

        private DynamicEnumerableFunction DirectTableQuery(string foreignKey, DynamicModel model)
        {
            return () => repository.All(foreignKey + " = @0", args: new[] { model.Expando.Id });
        }

        private DynamicEnumerableFunction ThroughTableQuery(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
        {
            return () => repository.Query(
                 @"
                  select {toTable}.* 
                  from {throughTable}
                  inner join {toTable}
                  on {throughTable}.{using} = {toTable}.Id
                  where {fromColumn} = @0"
                    .Replace("{fromColumn}", fromColumn)
                    .Replace("{toTable}", toTable)
                    .Replace("{throughTable}", throughTable)
                    .Replace("{using}", @using), model.Expando.Id);
        }
    }

    public class HasOne : Association
    {
        private DynamicRepository repository;

        public DynamicRepository Through { get; set; }

        public HasOne(DynamicRepository repository)
        {
            this.repository = repository;
        }

        public override void Init(dynamic model)
        {
            var foreignKey = model.GetType().Name + "Id";

            if (Through != null)
            {
                (model.Virtual as Prototype).SetMember(
                    MakeSingular(repository),
                    ThroughTableQuery(foreignKey, repository.GetType().Name, Through.GetType().Name, SigularizedIdFor(repository), model));
            }
            else
            {
                (model.Virtual as Prototype).SetMember(
                    MakeSingular(repository),
                    new DynamicFunction(() => repository.SingleWhere(foreignKey + " = @0", model.GetMember("Id"))));
            }
        }

        private DynamicFunction ThroughTableQuery(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
        {
            return () => repository.Query(
                @"
                select {toTable}.*
                from {throughTable}
                inner join {toTable}
                on {throughTable}.{using} = {toTable}.Id
                where {fromColumn} = @0"
                    .Replace("{toTable}", toTable)
                    .Replace("{throughTable}", throughTable)
                    .Replace("{using}", @using)
                    .Replace("{fromColumn}", fromColumn), model.Expando.Id as object)
                    .FirstOrDefault();
        }
    }

    public class BelongsTo : Association
    {
        private DynamicRepository repository;

        private string name;

        public BelongsTo(DynamicRepository repository)
        {
            this.repository = repository;
            name = MakeSingular(repository);
        }

        public override void Init(dynamic model)
        {
            (model.Virtual as Prototype).SetMember(
                name,
                new DynamicFunction(() => repository.Single(model.GetMember(SigularizedIdFor(repository)))));
        }
    }
}