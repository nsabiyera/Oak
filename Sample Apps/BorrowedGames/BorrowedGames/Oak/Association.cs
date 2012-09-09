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

    public class Association : Gemini
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

            options = (options as object).ToPrototype();

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
                .Replace("{inClause}", InClause(models, Id()));
        }

        public void AddReferenceBackToModel(dynamic association, dynamic model)
        {
            association.SetMember(model.GetType().Name, Model);
        }

        public string InClause(IEnumerable<dynamic> models, string member)
        {
            return string.Join(",", models.Select(s => string.Format("'{0}'", s.GetMember(member))));
        }

        public virtual void AddRepository(DynamicModels collection, DynamicRepository repository)
        {
            collection.SetMember("Repository", repository);
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

    public class EagerLoadMany
    {
        public DynamicModels Cache { get; set; }

        public bool ShouldDiscardCache(dynamic options)
        {
            if (options == null) options = new { discardCache = false };

            options = (options as object).ToPrototype();

            return options.discardCache;
        }

        public DynamicModels Execute(dynamic options,
            DynamicRepository repository,
            string associationName,
            string selectClause,
            IEnumerable<dynamic> models,
            string parentMemberName)
        {
            if (ShouldDiscardCache(options)) Cache = null;

            if (Cache != null) return Cache;

            var many = repository.Query(selectClause).ToList();

            foreach (var item in many)
            {
                var model = models.First(s => s.Id == item.GetMember(parentMemberName));

                item.SetMember(model.GetType().Name, model);
            }

            foreach (var model in models)
            {
                var assocation = model.AssociationNamed(associationName);

                var relatedTo = many.Where(s => s.GetMember(model.GetType().Name).Equals(model)).Select(s => s);

                assocation.EagerLoadMany.Cache = new DynamicModels(relatedTo);

                assocation.AddNewAssociationMethod(assocation.EagerLoadMany.Cache, model);
            }

            return new DynamicModels(many);
        }
    }

    public class HasMany : Association
    {
        public string ForeignKey { get; set; }

        public EagerLoadMany EagerLoadMany { get; set; }

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
            EagerLoadMany = new EagerLoadMany();

            ForeignKey = ForeignKey ?? ForeignKeyFor(model);

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

            entity.SetMember(ForeignKey, model.GetMember(Id()));

            return Repository.Projection(entity);
        }

        private DynamicFunctionWithParam Query(string foreignKey, dynamic model)
        {
            return (options) =>
            {
                if (EagerLoadMany.ShouldDiscardCache(options)) EagerLoadMany.Cache = null;

                if (EagerLoadMany.Cache != null) return EagerLoadMany.Cache;

                EagerLoadMany.Cache = new DynamicModels(Repository.Query(SelectClause(model)));

                AddNewAssociationMethod(EagerLoadMany.Cache, model);

                return EagerLoadMany.Cache;
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

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var query = SelectClause(models.ToArray());

            return EagerLoadMany.Execute(options, Repository, Named, query, models, ForeignKey);
        }

        private string SelectClause(params dynamic[] models)
        {
            return @"
                select {childTable}.* 
                from {childTable} 
                where {foreignKey} in ({inClause})"
                .Replace("{childTable}", TableName)
                .Replace("{foreignKey}", ForeignKey)
                .Replace("{inClause}", InClause(models, Id()));
        }
    }

    public class HasManyThrough : Association
    {
        string toTable;

        string throughTable;

        string resolvedForeignKey;

        DynamicRepository through;

        public EagerLoadMany EagerLoadMany { get; set; }

        public string ForeignKey { get; set; }

        public string FromColumn { get; set; }

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
            EagerLoadMany = new EagerLoadMany();

            FromColumn = FromColumn ?? ForeignKeyFor(model);

            toTable = Repository.GetType().Name;

            resolvedForeignKey = ForeignKey ?? ForeignKeyFor(Repository);

            AddAssociationMethod(model);

            Model = model;
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
                if (EagerLoadMany.ShouldDiscardCache(options)) EagerLoadMany.Cache = null;

                if (EagerLoadMany.Cache != null) return EagerLoadMany.Cache;

                var models = (Repository.Query(InnerJoinSelectClause(FromColumn, toTable, throughTable, resolvedForeignKey, model)) as IEnumerable<dynamic>).ToList();

                foreach (var m in models) AddReferenceBackToModel(m, model);

                EagerLoadMany.Cache = new DynamicModels(models);

                AddNewAssociationMethod(EagerLoadMany.Cache, model);

                return EagerLoadMany.Cache;
            };
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            string sql = InnerJoinSelectClause(FromColumn, toTable, throughTable, resolvedForeignKey, models.ToArray());

            return EagerLoadMany.Execute(options, Repository, Named, sql, models, FromColumn);
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
        public EagerLoadMany EagerLoadMany { get; set; }

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
            EagerLoadMany = new EagerLoadMany();

            throughTable = CrossRefenceTable ?? throughTable;

            fromColumn = FromColumn ?? ForeignKeyFor(model);

            toTable = Repository.TableName;

            resolvedForeignKey = ForeignKey ?? ForeignKeyFor(Repository);

            AddAssociationMethods(model);

            Model = model;
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
                if (EagerLoadMany.ShouldDiscardCache(options)) EagerLoadMany.Cache = null;

                if (EagerLoadMany.Cache != null) return EagerLoadMany.Cache;

                string innerJoinSelectClause = InnerJoinSelectClause(fromColumn, toTable, throughTable, resolvedForeignKey, model);

                var models = (Repository.Query(innerJoinSelectClause) as IEnumerable<dynamic>).ToList();

                foreach (var m in models) AddReferenceBackToModel(m, model);

                EagerLoadMany.Cache = new DynamicModels(models);

                AddNewAssociationMethod(EagerLoadMany.Cache, model);

                AddRepository(EagerLoadMany.Cache, new DynamicRepository(throughTable));

                return EagerLoadMany.Cache;
            };
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var sql = InnerJoinSelectClause(fromColumn, toTable, throughTable, resolvedForeignKey, models.ToArray());

            return EagerLoadMany.Execute(options, Repository, Named, sql, models, fromColumn);
        }
    }

    public class HasOne : Association
    {
        public string ForeignKey { get; set; }

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

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var foreignKeyName = ForeignKeyName(models.First());

            var ones = @"
            select * from {fromTable} 
            where {foreignKey}
            in ({inClause})"
                .Replace("{fromTable}", Repository.TableName)
                .Replace("{foreignKey}", foreignKeyName)
                .Replace("{inClause}", InClause(models,Id()));

            var onesResult = new List<dynamic>(Repository.Query(ones));

            foreach (var item in onesResult)
            {
                var model = models.FirstOrDefault(s => s.Id == item.GetMember(foreignKeyName));

                if (model != null) //need to add a test of why this cant be null, this is here if the entity doesn't have an assocation reference
                {
                    var association = model.AssociationNamed(Named);

                    association.Model = item;

                    item.SetMember(model.GetType().Name, model);
                }
            }

            return new DynamicModels(onesResult);
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
                new List<dynamic>() { model })();

            return Model;
        }

        public string InnerJoinSelectClause(string fromColumn, string toTable, string throughTable, string @using, List<dynamic> models)
        {
            return @"
                select {toTable}.*, {throughTable}.{fromColumn}
                from {throughTable}
                inner join {toTable}
                on {throughTable}.{using} = {toTable}.Id
                where {fromColumn} in ({in})"
                    .Replace("{toTable}", toTable)
                    .Replace("{throughTable}", throughTable)
                    .Replace("{using}", @using)
                    .Replace("{fromColumn}", fromColumn)
                    .Replace("{in}", InClause(models, Id()));
        }

        private DynamicFunction Query(string fromColumn, string toTable, string throughTable, string @using, List<dynamic> models)
        {
            return () => Repository.Query(InnerJoinSelectClause(fromColumn, toTable, throughTable, @using, models)).FirstOrDefault();
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var foreignKeyName = ForeignKeyName(models.First());

            var sql = InnerJoinSelectClause(foreignKeyName,
                Repository.GetType().Name,
                through.GetType().Name,
                ForeignKeyFor(Repository), models.ToList());

            var onesThrough = new List<dynamic>(Repository.Query(sql));

            foreach (var item in onesThrough)
            {
                var model = models.FirstOrDefault(s => s.Id == item.GetMember(foreignKeyName));

                if (model != null)  //need to add a test of why this cant be null, this is here if the entity doesn't have an assocation reference
                {
                    var association = model.AssociationNamed(Named);

                    association.Model = model;

                    item.SetMember(model.GetType().Name, model);    
                }
            }

            return new DynamicModels(onesThrough);
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
            ForeignKey = string.IsNullOrEmpty(ForeignKey) ? ForeignKeyFor(Repository) : ForeignKey;

            PrimaryKey = string.IsNullOrEmpty(PrimaryKey) ? "Id" : PrimaryKey;

            model.SetMember(
                Named,
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var ones = @"
            select * from {fromTable} 
            where {primaryKey}
            in ({inClause})"
                .Replace("{fromTable}", Repository.TableName)
                .Replace("{primaryKey}", PrimaryKey)
                .Replace("{inClause}", InClause(models, ForeignKey));

            var belongsResult = new List<dynamic>(Repository.Query(ones));

            foreach (var item in belongsResult)
            {
                var model = models.FirstOrDefault(s => item.GetMember(PrimaryKey) == s.GetMember(ForeignKey));
                
                if(model != null) //need to add a test of why this cant be null, this is here if the entity doesn't have an assocation reference
                {
                    var association = model.AssociationNamed(Named);

                    association.Model = item;

                    item.SetMember(model.GetType().Name, model);
                }
            }

            return new DynamicModels(belongsResult);
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            string whereClause = string.Format("{0} = @0", PrimaryKey);

            Model = Repository.SingleWhere(whereClause, model.GetMember(ForeignKey));

            return Model;
        }
    }
}
