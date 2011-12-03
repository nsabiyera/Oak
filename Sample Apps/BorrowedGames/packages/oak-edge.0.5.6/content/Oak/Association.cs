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
        List<dynamic> referencedAssociations = new List<dynamic>();

        public MixInAssociation(DynamicModel mixWith)
        {
            if (!SupportsAssociations(mixWith)) return;

            IEnumerable<dynamic> associations = (mixWith as dynamic).Associates();

            foreach (dynamic association in associations)
            {
                referencedAssociations.Add(association);
                association.Init(mixWith);
            }

            mixWith.SetUnTrackedMember("AssociationNamed", new DynamicFunctionWithParam(AssociationNamed));
        }

        public bool SupportsAssociations(DynamicModel mixWith)
        {
            return mixWith.GetType().GetMethod("Associates") != null;
        }

        public dynamic AssociationNamed(dynamic name)
        {
            return referencedAssociations.FirstOrDefault(s => s.Named == name);
        }
    }

    public class Association
    {
        public string Named { get; set; }

        public DynamicRepository Repository { get; set; }

        public string TableName
        {
            get { return Repository.TableName; }
        }

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

        public bool DiscardCache(dynamic options)
        {
            if (options == null) options = new { discardCache = false };

            options = (options as object).ToExpando();

            return options.discardCache;
        }

        public string InClause(IEnumerable<dynamic> models)
        {
            return string.Join(",", models.Select(s => string.Format("'{0}'", s.GetMember(Id()))));
        }
    }

    public class HasMany : Association
    {
        DynamicModels cachedCollection;

        public string ForeignKey { get; set; }

        DynamicModels selectManyRelatedToCache;

        public HasMany(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public HasMany(DynamicRepository repository, string named)
        {
            this.Repository = repository;

            this.Named = named ?? repository.GetType().Name;
        }

        public void Init(DynamicModel model)
        {
            ForeignKey = ForeignKeyFor(model);

            var toTable = Repository.GetType().Name;

            AddAssociationMethods(model, ForeignKey, toTable);
        }

        private void AddAssociationMethods(DynamicModel model, string fromColumn, string toTable)
        {
            model.SetUnTrackedMember(Named, Query(fromColumn, model));

            model.SetUnTrackedMember(Singular(Named) + "Ids", QueryIds(fromColumn, model));
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

            return Repository.Projection(entity);
        }

        private DynamicFunctionWithParam Query(string foreignKey, dynamic model)
        {
            return (options) =>
            {
                if (DiscardCache(options)) cachedCollection = null;

                if (cachedCollection != null) return cachedCollection;

                cachedCollection = new DynamicModels(Repository.Query(SelectClause(model)));

                AddNewAssociationMethod(cachedCollection, model);

                return cachedCollection;
            };
        }

        private DynamicFunction QueryIds(string foreignKey, dynamic model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (Query(foreignKey, model) as DynamicFunctionWithParam).Invoke(null);

                return models.Select(s => s.Id).ToList();
            };
        }

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            if (DiscardCache(options)) selectManyRelatedToCache = null;
            
            if(selectManyRelatedToCache != null) return selectManyRelatedToCache;

            var query = SelectClause(models.ToArray());

            var many = Repository.Query(query).ToList();

            foreach (var item in many)
            {
                var model = models.First(s => s.Id == item.GetMember(ForeignKey));

                item.SetMember(model.GetType().Name, new DynamicFunction(() => model));
            }

            selectManyRelatedToCache = new DynamicModels(many);

            return selectManyRelatedToCache;
        }

        private string SelectClause(params dynamic[] models)
        {
            return @"
                select {childTable}.* 
                from {childTable} 
                where {foreignKey} in ({inClause})"
                .Replace("{childTable}", TableName)
                .Replace("{foreignKey}", ForeignKey)
                .Replace("{inClause}", InClause(models));
        }
    }

    public class HasManyThrough : Association
    {
        string fromColumn;

        string toTable;

        string throughTable;

        string resolvedForeignKey;

        DynamicRepository through;

        DynamicModels cachedCollection;

        DynamicModels selectManyRelatedToCache;

        public string ForeignKey { get; set; }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through)
            : this(repository, through, null)
        {

        }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through, string named)
        {
            this.Repository = repository;

            this.through = through;

            this.throughTable = through.GetType().Name;

            this.Named = named ?? repository.GetType().Name;
        }

        public void Init(DynamicModel model)
        {
            fromColumn = ForeignKeyFor(model);

            toTable = Repository.GetType().Name;

            resolvedForeignKey = ForeignKey ?? ForeignKeyFor(Repository);

            AddAssociationMethod(model);
        }

        private void AddAssociationMethod(DynamicModel model)
        {
            model.SetUnTrackedMember(
                Named,
                Query(model));

            model.SetUnTrackedMember(
                Singular(Named) + "Ids",
                QueryIds(model));
        }

        private DynamicFunctionWithParam Query(DynamicModel model)
        {
            return (options) =>
            {
                if (DiscardCache(options)) cachedCollection = null;

                if (cachedCollection != null) return cachedCollection;

                cachedCollection = new DynamicModels(Repository.Query(SelectClause(model)));

                AddNewAssociationMethod(cachedCollection, model);

                return cachedCollection;
            };
        }

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            if (DiscardCache(options)) selectManyRelatedToCache = null;

            if(selectManyRelatedToCache != null) return selectManyRelatedToCache;

            var many = Repository.Query(SelectClause(models.ToArray())).ToList();

            foreach (var item in many)
            {
                var model = models.First(s => s.Id == item.GetMember(fromColumn));

                item.SetMember(model.GetType().Name, new DynamicFunction(() => model));
            }

            selectManyRelatedToCache = new DynamicModels(many);

            return selectManyRelatedToCache;
        }

        private string SelectClause(params dynamic[] models)
        {
            return @"
            select {toTable}.*, {throughTable}.{fromColumn}
            from {throughTable}
            inner join {toTable}
            on {throughTable}.{using} = {toTable}.Id
            where {throughTable}.{fromColumn} in ({inClause})"
                .Replace("{fromColumn}", fromColumn)
                .Replace("{toTable}", toTable)
                .Replace("{throughTable}", throughTable)
                .Replace("{using}", resolvedForeignKey)
                .Replace("{inClause}", InClause(models));
        }
        
        private void AddNewAssociationMethod(DynamicModels collection, DynamicModel model)
        {
            collection.SetMember(
                "New",
                new DynamicFunctionWithParam(attributes =>
                {
                    return EntityFor(attributes);
                }));
        }

        private dynamic EntityFor(dynamic attributes)
        {
            var entity = new Gemini(attributes);

            return Repository.Projection(entity);
        }

        private DynamicFunction QueryIds(DynamicModel model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (Query(model) as DynamicFunctionWithParam).Invoke(null);

                return models.Select(s => s.Id).ToList();
            };
        }
    }

    public class HasOne : Association
    {
        public string ForeignKey { get; set; }

        public HasOne(DynamicRepository repository)
        {
            this.Repository = repository;
        }

        public void Init(DynamicModel model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;

            model.SetUnTrackedMember(
                Singular(Repository),
                new DynamicFunction(() => Repository.SingleWhere(foreignKeyName + " = @0", model.GetMember(Id()))));
        }
    }

    public class HasOneThrough : Association
    {
        private DynamicRepository through;

        public string ForeignKey { get; set; }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through)
        {
            this.Repository = repository;
            this.through = through;
        }

        public void Init(DynamicModel model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;

            model.SetUnTrackedMember(
                Singular(Repository),
                Query(foreignKeyName, Repository.GetType().Name, through.GetType().Name, ForeignKeyFor(Repository), model));
        }

        private DynamicFunction Query(string fromColumn, string toTable, string throughTable, string @using, DynamicModel model)
        {
            return () => Repository.Query(
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
        public string ForeignKey { get; set; }

        public string PrimaryKey { get; set; }

        public BelongsTo(DynamicRepository repository)
        {
            this.Repository = repository;
            Named = Singular(repository);
        }

        public void Init(DynamicModel model)
        {
            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(Repository) : ForeignKey;

            string primaryKeyName = string.IsNullOrEmpty(PrimaryKey) ? "Id" : PrimaryKey;

            string whereClause = string.Format("{0} = @0", primaryKeyName);

            model.SetUnTrackedMember(
                Named,
                new DynamicFunction(() => Repository.SingleWhere(whereClause, model.GetMember(foreignKeyName))));
        }
    }
}
