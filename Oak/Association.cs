using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace Oak
{
    public delegate dynamic DynamicMethod();

    public delegate IEnumerable<dynamic> DynamicEnumerableMethod();

    public class Association
    {
        public virtual void Init(DynamicModel model)
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

        public override void Init(DynamicModel model)
        {
            var fromColumn = model.GetType().Name + "Id";

            var toTable = repository.GetType().Name;

            if(Through == null)
            {
                (model.Virtual as Prototype).SetValueFor(
                    named,
                    DirectTableQuery(fromColumn, model));    
            }
            else
            {
                (model.Virtual as Prototype).SetValueFor(
                    named,
                    ThroughTableQuery(fromColumn, toTable, Through.GetType().Name, Using ?? SigularizedIdFor(repository), model));
            }
        }

        private DynamicEnumerableMethod DirectTableQuery(string foreignKey, DynamicModel model)
        {
            return () => repository.All(foreignKey + " = @0", args: new[] { model.Expando.Id });
        }

        private DynamicEnumerableMethod ThroughTableQuery(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
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

        public HasOne(DynamicRepository repository)
        {
            this.repository = repository;
        }

        public override void Init(DynamicModel model)
        {
            var foreignKey = model.GetType().Name + "Id";

            (model.Virtual as Prototype).SetValueFor(
                MakeSingular(repository),
                new DynamicMethod(() => repository.SingleWhere(foreignKey + " = @0", model.GetValueFor("Id"))));
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

        public override void Init(DynamicModel model)
        {
            (model.Virtual as Prototype).SetValueFor(
                name,
                new DynamicMethod(() => repository.Single(model.GetValueFor(SigularizedIdFor(repository)))));
        }
    }
}