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

            if (o is string) name = o as string;

            if (!name.EndsWith("s")) return name;

            return name.Substring(0, name.Length - 1);
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

            AddAssociationMethods(model, fromColumn, toTable);
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

        private void AddAssociationMethods(DynamicModel model, string fromColumn, string toTable)
        {
            model.SetUnTrackedMember(named, Query(fromColumn, model));

            model.SetUnTrackedMember(Singular(named) + "Ids", QueryIds(fromColumn, model));
        }

        private DynamicFunction Query(string foreignKey, dynamic model)
        {
            return () =>
            {
                var collection = new DynamicModels(repository.All(foreignKey + " = @0", args: new[] { model.Expando.Id }));

                AddNewAssociationMethod(collection, model);

                return collection;
            };
        }

        private DynamicFunction QueryIds(string foreignKey, dynamic model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (Query(foreignKey, model) as DynamicFunction).Invoke();

                return models.Select(s => s.Id).ToList();
            };
        }
    }

    public class HasManyThrough : Association
    {
        private DynamicRepository repository;

        private DynamicRepository through;

        private string named;

        public string Using { get; set; }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through)
            : this(repository, through, null)
        {

        }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through, string named)
        {
            this.repository = repository;

            this.through = through;

            this.named = named ?? repository.GetType().Name;
        }

        public void Init(dynamic model)
        {
            var fromColumn = ForeignKeyFor(model);

            var toTable = repository.GetType().Name;

            AddAssociationMethod(model, fromColumn, toTable);
        }

        private void AddAssociationMethod(DynamicModel model, string fromColumn, string toTable)
        {
            model.SetUnTrackedMember(
                named,
                Query(fromColumn, toTable, through.GetType().Name, Using ?? ForeignKeyFor(repository), model));

            model.SetUnTrackedMember(
                Singular(named) + "Ids",
                QueryIds(fromColumn, toTable, through.GetType().Name, Using ?? ForeignKeyFor(repository), model));
        }

        private DynamicFunction Query(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
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

        private DynamicFunction QueryIds(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (Query(fromColumn, toTable, throughTable, @using, model) as DynamicFunction).Invoke();

                return models.Select(s => s.Id).ToList();
            };
        }
    }

    public class HasOne : Association
    {
        private DynamicRepository repository;

        public string ForeignKey { get; set; }

        public HasOne(DynamicRepository repository)
        {
            this.repository = repository;
        }

        public void Init(dynamic model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;

            (model as DynamicModel).SetUnTrackedMember(
                    Singular(repository),
                    new DynamicFunction(() => repository.SingleWhere(foreignKeyName + " = @0", model.GetMember(Id()))));
        }
    }

    public class HasOneThrough : Association
    {
        private DynamicRepository repository;

        private DynamicRepository through;

        public string ForeignKey { get; set; }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through)
        {
            this.repository = repository;
            this.through = through;
        }

        public void Init(dynamic model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;

            (model as DynamicModel).SetUnTrackedMember(
                    Singular(repository),
                    Query(foreignKeyName, repository.GetType().Name, through.GetType().Name, ForeignKeyFor(repository), model));
        }

        private DynamicFunction Query(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
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

        public string ForeignKey { get; set; }

        public string PrimaryKey { get; set; }

        public BelongsTo(DynamicRepository repository)
        {
            this.repository = repository;
            name = Singular(repository);
        }

        public void Init(dynamic model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(repository) : ForeignKey;
            string primaryKeyName = (string.IsNullOrEmpty(PrimaryKey) ? "Id" : PrimaryKey);

            string whereClause = string.Format("{0} = @0", primaryKeyName);

            (model as DynamicModel).SetUnTrackedMember(
                name,
                new DynamicFunction(() => repository.SingleWhere(whereClause, model.GetMember(foreignKeyName)
                                                    )));
        }
    }
}