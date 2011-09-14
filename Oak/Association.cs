using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace Oak
{
    public class Association
    {
        public virtual void Init(DynamicModel model)
        {

        }

        public string MakeSingular(object o)
        {
            return o.GetType().Name.Substring(0, o.GetType().Name.Length - 1);
        }

        public string IdFor(object o)
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
                    ThroughTableQuery(fromColumn, toTable, Through.GetType().Name, Using ?? IdFor(repository), model));
            }
        }

        private Func<IEnumerable<dynamic>> DirectTableQuery(string foreignKey, DynamicModel model)
        {
            return () => repository.All(foreignKey + " = @0", args: new[] { model.Expando.Id });
        }

        private Func<IEnumerable<dynamic>> ThroughTableQuery(string fromColumn, string toTable, string throughTable, string joinColumn, DynamicModel model)
        {
            return () => repository.Query(
                string.Format(
                 @"
                  select {1}.* 
                  from {2}
                  inner join {1}
                  on {2}.{3} = {1}.Id
                  where {0} = @0", fromColumn, toTable, throughTable, joinColumn), model.Expando.Id);
        }
    }

    public class HasOne : Association
    {
        private DynamicRepository repository;

        private string name;

        public HasOne(DynamicRepository repository)
        {
            this.repository = repository;
            name = MakeSingular(repository);
        }

        public override void Init(DynamicModel model)
        {
            (model.Virtual as Prototype).SetValueFor(
                name,
                new Func<dynamic>(() => repository.Single(model.GetValueFor(IdFor(repository)))));
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
                new Func<dynamic>(() => repository.Single(model.GetValueFor(IdFor(repository)))));
        }
    }
}