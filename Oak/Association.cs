using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using System.Dynamic;

namespace Oak
{
    public class MixInAssociation
    {
        public MixInAssociation(DynamicModel mixWith)
        {
            if (!SupportsAssociations(mixWith)) return;
                
            IEnumerable<dynamic> associations = (mixWith as dynamic).Associates();

            foreach (dynamic association in associations) association.Init(mixWith);
        }

        public bool SupportsAssociations(DynamicModel mixWith)
        {
            return mixWith.GetType().GetMethod("Associates") != null;
        }
    }

    public class Association
    {
        public string Singular(object o)
        {
            var name = o.GetType().Name;

            if (!name.EndsWith("s")) return name;

            return name.Substring(0, o.GetType().Name.Length - 1);
        }

        public string ForeignKeyFor(object o)
        {
            return Singular(o) + Id();
        }

        public string Id()
        {
            return "Id";
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

        public void Init(dynamic model)
        {
            var fromColumn = ForeignKeyFor(model);

            var toTable = repository.GetType().Name;

            AddAssociationMethod(model, fromColumn, toTable);
        }

        private void AddNewAssociationMethod(DynamicModels collection, DynamicModel model)
        {
            collection.SetMember(
                "New",
                new DynamicFunctionWithParam(attributes =>
                {
                    return EntityFor(model, attributes);
                }));
        }

        private dynamic EntityFor(DynamicModel model, dynamic attributes)
        {
            var entity = new Gemini(attributes);

            entity.SetMember(ForeignKeyFor(model), model.GetMember(Id()));

            return repository.Projection(entity);
        }

        private void AddAssociationMethod(DynamicModel model, string fromColumn, string toTable)
        {
            if (Through == null)
            {
                model.SetUnTrackedMember(
                    named,
                    DirectTableQuery(fromColumn, model));
            }
            else
            {
                model.SetUnTrackedMember(
                    named,
                    ThroughTableQuery(fromColumn, toTable, Through.GetType().Name, Using ?? ForeignKeyFor(repository), model));
            }
        }

        private DynamicFunction DirectTableQuery(string foreignKey, dynamic model)
        {
            return () => 
            {
                var collection = new DynamicModels(repository.All(foreignKey + " = @0", args: new[] { model.Expando.Id }));

                AddNewAssociationMethod(collection, model);

                return collection;
            };
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

        public void Init(dynamic model)
        {
            var foreignKey = ForeignKeyFor(model);

            if (Through != null)
            {
                (model as DynamicModel).SetUnTrackedMember(
                    Singular(repository),
                    ThroughTableQuery(foreignKey, repository.GetType().Name, Through.GetType().Name, ForeignKeyFor(repository), model));
            }
            else
            {
                (model as DynamicModel).SetUnTrackedMember(
                    Singular(repository),
                    new DynamicFunction(() => repository.SingleWhere(foreignKey + " = @0", model.GetMember(Id()))));
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
            name = Singular(repository);
        }

        public void Init(dynamic model)
        {
            (model as DynamicModel).SetUnTrackedMember(
                name,
                new DynamicFunction(() => repository.Single(model.GetMember(ForeignKeyFor(repository)))));
        }
    }
}