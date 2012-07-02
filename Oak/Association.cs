using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using System.Dynamic;

namespace Oak
{
    public class Associations
    {
        List<dynamic> referencedAssociations = new List<dynamic>();

        public Associations(dynamic mixWith)
        {
            if (!SupportsAssociations(mixWith)) return;

            IEnumerable<dynamic> associations = (mixWith as dynamic).Associates();

            foreach (dynamic association in associations)
            {
                referencedAssociations.Add(association);

                association.Init(mixWith);
            }

            mixWith.SetMember("AssociationNamed", new DynamicFunctionWithParam(AssociationNamed));
        }

        public bool SupportsAssociations(dynamic mixWith)
        {
            return mixWith.GetType().GetMethod("Associates") != null || mixWith.RespondsTo("Associates");
        }

        public dynamic AssociationNamed(dynamic collectionName)
        {
            var association = referencedAssociations.FirstOrDefault(s => s.Named == collectionName || s.Named == Singularize(collectionName));

            if (association == null) throw new InvalidOperationException("No association named " + collectionName + " exists.");

            return association;
        }

        public string Singularize(string collectionName)
        {
            return collectionName.Substring(0, collectionName.Length - 1);
        }
    }

    public class Association
    {
        public string Named { get; set; }

        public DynamicRepository Repository { get; set; }

        public dynamic Model { get; set; }

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

        public string InnerJoinSelectClause(string fromColumn, string toTable, string throughTable, string foreignKey, params dynamic[] models)
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
                .Replace("{using}", foreignKey)
                .Replace("{inClause}", InClause(models));
        }

        public void AddReferenceBackToModel(dynamic association, dynamic model)
        {
            association.SetMember(model.GetType().Name, ModelReference);
        }

        public DynamicFunction ModelReference { get; set; }

        public string InClause(IEnumerable<dynamic> models)
        {
            return string.Join(",", models.Select(s => string.Format("'{0}'", s.GetMember(Id()))));
        }

        public virtual void AddNewAssociationMethod(DynamicModels collection, dynamic model)
        {
            collection.SetMember(
                "New",
                new DynamicFunctionWithParam(attributes =>
                {
                    return EntityFor(attributes);
                }));
        }

        public dynamic EntityFor(dynamic attributes)
        {
            var entity = new Gemini(attributes);

            return Repository.Projection(entity);
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

        public void Init(dynamic model)
        {
            ForeignKey = ForeignKeyFor(model);

            var toTable = Repository.GetType().Name;

            AddAssociationMethods(model, ForeignKey, toTable);
        }

        private void AddAssociationMethods(dynamic model, string fromColumn, string toTable)
        {
            model.SetMember(Named, Query(fromColumn, model));

            model.SetMember(Singular(Named) + "Ids", QueryIds(fromColumn, model));
        }

        public override void AddNewAssociationMethod(DynamicModels collection, dynamic model)
        {
            collection.SetMember(
                "New",
                new DynamicFunctionWithParam(attributes =>
                {
                    return EntityFor(model, attributes);
                }));
        }

        private dynamic EntityFor(dynamic model, dynamic attributes)
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

            if (selectManyRelatedToCache != null) return selectManyRelatedToCache;

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

        public void Init(dynamic model)
        {
            fromColumn = ForeignKeyFor(model);

            toTable = Repository.GetType().Name;

            resolvedForeignKey = ForeignKey ?? ForeignKeyFor(Repository);

            AddAssociationMethod(model);

            ModelReference = new DynamicFunction(() => model);
        }

        private void AddAssociationMethod(dynamic model)
        {
            model.SetMember(
                Named,
                InnerJoinFor(model));

            model.SetMember(
                Singular(Named) + "Ids",
                QueryIds(model));
        }

        private DynamicFunctionWithParam InnerJoinFor(dynamic model)
        {
            return (options) =>
            {
                if (DiscardCache(options)) cachedCollection = null;

                if (cachedCollection != null) return cachedCollection;

                var models = (Repository.Query(InnerJoinSelectClause(fromColumn, toTable, throughTable, resolvedForeignKey, model)) as IEnumerable<dynamic>).ToList();

                foreach (var m in models) AddReferenceBackToModel(m, model);

                cachedCollection = new DynamicModels(models);

                AddNewAssociationMethod(cachedCollection, model);

                return cachedCollection;
            };
        }

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            if (DiscardCache(options)) selectManyRelatedToCache = null;

            if (selectManyRelatedToCache != null) return selectManyRelatedToCache;

            var many = Repository.Query(InnerJoinSelectClause(fromColumn, toTable, throughTable, resolvedForeignKey, models.ToArray())).ToList();

            foreach (var item in many)
            {
                var model = models.First(s => s.Id == item.GetMember(fromColumn));

                item.SetMember(model.GetType().Name, new DynamicFunction(() => model));
            }

            selectManyRelatedToCache = new DynamicModels(many);

            return selectManyRelatedToCache;
        }

        private DynamicFunction QueryIds(dynamic model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (InnerJoinFor(model) as DynamicFunctionWithParam).Invoke(null);

                return models.Select(s => s.Id).ToList();
            };
        }
    }

    public class HasManyAndBelongsTo : Association
    {
        dynamic cachedCollection;

        string throughTable;

        string fromColumn;

        DynamicRepository reference;

        string resolvedForeignKey;

        string toTable;

        public string CrossRefenceTable { get; set; }

        public string ForeignKey { get; set; }

        public string FromColumn { get; set; }

        public HasManyAndBelongsTo(DynamicRepository repository, DynamicRepository reference)
        {
            Repository = repository;

            this.reference = reference;

            var sorted = new[] { repository.TableName, reference.TableName }.OrderBy(s => s);

            throughTable = sorted.First() + sorted.Last();

            Named = repository.GetType().Name;
        }

        public void Init(dynamic model)
        {
            throughTable = CrossRefenceTable ?? throughTable;

            fromColumn = FromColumn ?? ForeignKeyFor(model);

            toTable = Repository.TableName;

            resolvedForeignKey = ForeignKey ?? ForeignKeyFor(Repository);

            AddAssociationMethods(model);

            ModelReference = new DynamicFunction(() => model);
        }

        public void AddAssociationMethods(dynamic model)
        {
            model.SetMember(
                Named,
                InnerJoinFor(model));

            model.SetMember(
                Singular(Named) + "Ids",
                QueryIds(model));
        }

        private DynamicFunction QueryIds(dynamic model)
        {
            return () =>
            {
                IEnumerable<dynamic> models = (InnerJoinFor(model) as DynamicFunctionWithParam).Invoke(null);

                return models.Select(s => s.Id).ToList();
            };
        }

        private DynamicFunctionWithParam InnerJoinFor(dynamic model)
        {
            return (options) =>
            {
                if (DiscardCache(options)) cachedCollection = null;

                if (cachedCollection != null) return cachedCollection;

                string innerJoinSelectClause = InnerJoinSelectClause(fromColumn, toTable, throughTable, resolvedForeignKey, model);

                var models = (Repository.Query(innerJoinSelectClause) as IEnumerable<dynamic>).ToList();

                foreach (var m in models) AddReferenceBackToModel(m, model);

                cachedCollection = new DynamicModels(models);

                AddNewAssociationMethod(cachedCollection, model);

                return cachedCollection;
            };
        }
    }

    public class HasOne : Association
    {
        public string ForeignKey { get; set; }

        public dynamic HasOneModel { get; set; }

        public HasOne(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public HasOne(DynamicRepository repository, string named)
        {
            this.Repository = repository;

            Named = named ?? Singular(Repository);
        }

        public void Init(dynamic model)
        {
            model.SetMember(
                Named,
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public string ForeignKeyName(dynamic model)
        {
            return string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;
        }

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            List<dynamic> collection = new List<dynamic>();

            models.ForEach(s =>
            {
                var hasOne = s.GetMember(Named)(new { discardCache = false });
                hasOne.SetMember(s.GetType().Name, new DynamicFunction(() => s));
                collection.Add(hasOne);
            });

            return new DynamicModels(collection);
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            Model = Repository.SingleWhere(ForeignKeyName(model) + " = @0", model.GetMember(Id()));

            return Model;
        }
    }

    public class HasOneThrough : Association
    {
        private DynamicRepository through;

        public string ForeignKey { get; set; }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through)
            : this(repository, through, null)
        {
        }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through, string named)
        {
            this.Repository = repository;
            this.through = through;
            Named = named ?? Singular(Repository);
        }

        public void Init(dynamic model)
        {
            model.SetMember(
                Singular(Repository),
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public string ForeignKeyName(dynamic model)
        {
            return string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(model) : ForeignKey;
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            Model = Query(ForeignKeyName(model),
                Repository.GetType().Name,
                through.GetType().Name,
                ForeignKeyFor(Repository),
                model)();

            return Model;
        }

        private DynamicFunction Query(string fromColumn, string toTable, string throughTable, string @using, dynamic model)
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

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            List<dynamic> collection = new List<dynamic>();

            models.ForEach(s =>
            {
                var hasOne = s.GetMember(Named)(new { discardCache = false });
                hasOne.SetMember(s.GetType().Name, new DynamicFunction(() => s));
                collection.Add(hasOne);
            });

            return new DynamicModels(collection);
        }
    }

    public class BelongsTo : Association
    {
        public string ForeignKey { get; set; }

        public string PrimaryKey { get; set; }

        public BelongsTo(DynamicRepository repository)
            : this(repository, null)
        {
            
        }

        public BelongsTo(DynamicRepository repository, string named)
        {
            this.Repository = repository;
            Named = named ?? Singular(repository);
        }

        public void Init(dynamic model)
        {
            model.SetMember(
                Named,
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public IEnumerable<dynamic> SelectManyRelatedTo(IEnumerable<dynamic> models, dynamic options)
        {
            List<dynamic> collection = new List<dynamic>();

            models.ForEach(s =>
            {
                var belongsTo = s.GetMember(Named)(new { discardCache = false });
                belongsTo.SetMember(s.GetType().Name, new DynamicFunction(() => s));
                collection.Add(belongsTo);
            });

            return new DynamicModels(collection);
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            string foreignKeyName = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(Repository) : ForeignKey;

            string primaryKeyName = string.IsNullOrEmpty(PrimaryKey) ? "Id" : PrimaryKey;

            string whereClause = string.Format("{0} = @0", primaryKeyName);

            Model = Repository.SingleWhere(whereClause, model.GetMember(foreignKeyName));

            return Model;
        }
    }
}
