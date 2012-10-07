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
            var association = referencedAssociations.FirstOrDefault(s => s.MethodName == collectionName || s.MethodName == Singularize(collectionName));

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
        public string MethodName { get; set; }

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

        public string SigularId(object o)
        {
            return Singular(o) + Id();
        }

        public string Id()
        {
            return Repository.PrimaryKeyField;
        }

        public bool DiscardCache(dynamic options)
        {
            if (options == null) options = new { discardCache = false };

            options = (options as object).ToPrototype();

            return options.discardCache;
        }

        public string InnerJoinSelectClause(string xRefFromColumn, 
            string toTable, 
            string xRefTable, 
            string xRefToColumn, 
            string toTableColumn,
            string idProperty,
            params dynamic[] models)
        {
            return @"
            select {toTable}.*, {xRefTable}.{xRefFromColumn}
            from {xRefTable}
            inner join {toTable}
            on {xRefTable}.{xRefToColumn} = {toTable}.{toTableColumn}
            where {xRefTable}.{xRefFromColumn} in ({inClause})"
                .Replace("{xRefFromColumn}", xRefFromColumn)
                .Replace("{toTableColumn}", toTableColumn)
                .Replace("{toTable}", toTable)
                .Replace("{xRefTable}", xRefTable)
                .Replace("{xRefToColumn}", xRefToColumn)
                .Replace("{inClause}", InClause(models, idProperty));
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

        public string PropertyContainingIdValue { get; set; }

        public EagerLoadMany EagerLoadMany { get; set; }

        public HasMany(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public HasMany(DynamicRepository repository, string methodName)
        {
            this.Repository = repository;

            this.MethodName = methodName ?? repository.TableName;
        }

        public void Init(dynamic model)
        {
            EagerLoadMany = new EagerLoadMany();

            ForeignKey = ForeignKey ?? SigularId(model);

            PropertyContainingIdValue = PropertyContainingIdValue ?? Id();

            var toTable = Repository.TableName;

            AddAssociationMethods(model, ForeignKey);
        }

        private void AddAssociationMethods(dynamic model, string fromColumn)
        {
            model.SetMember(MethodName, Query(fromColumn, model));

            model.SetMember(Singular(MethodName) + "Ids", QueryIds(fromColumn, model));
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

            entity.SetMember(ForeignKey, model.GetMember(PropertyContainingIdValue));

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

            return EagerLoadMany.Execute(options, Repository, MethodName, query, models, ForeignKey);
        }

        private string SelectClause(params dynamic[] models)
        {
            return @"
                select {childTable}.* 
                from {childTable} 
                where {foreignKey} in ({inClause})"
                .Replace("{childTable}", TableName)
                .Replace("{foreignKey}", ForeignKey)
                .Replace("{inClause}", InClause(models, PropertyContainingIdValue));
        }
    }

    public class HasManyThrough : Association
    {
        string toTable;

        string throughTable;

        DynamicRepository through;

        public EagerLoadMany EagerLoadMany { get; set; }

        public string XRefToColumn { get; set; }

        public string XRefFromColumn { get; set; }

        public string ToTableColumn { get; set; }

        public string PropertyContainingIdValue { get; set; }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through)
            : this(repository, through, null)
        {

        }

        public HasManyThrough(DynamicRepository repository, DynamicRepository through, string methodName)
        {
            this.Repository = repository;

            this.through = through;

            this.throughTable = through.TableName;

            this.MethodName = methodName ?? repository.TableName;
        }

        public void Init(dynamic model)
        {
            EagerLoadMany = new EagerLoadMany();

            toTable = Repository.TableName;

            XRefFromColumn = XRefFromColumn ?? SigularId(model);

            XRefToColumn = XRefToColumn ?? SigularId(Repository);

            ToTableColumn = ToTableColumn ?? Id();

            PropertyContainingIdValue = PropertyContainingIdValue ?? Id();

            AddAssociationMethod(model);

            Model = model;
        }

        private void AddAssociationMethod(dynamic model)
        {
            model.SetMember(
                MethodName,
                InnerJoinFor(model));

            model.SetMember(
                Singular(MethodName) + "Ids",
                QueryIds(model));
        }

        private DynamicFunctionWithParam InnerJoinFor(dynamic model)
        {
            return (options) =>
            {
                if (EagerLoadMany.ShouldDiscardCache(options)) EagerLoadMany.Cache = null;

                if (EagerLoadMany.Cache != null) return EagerLoadMany.Cache;

                var models = (Repository.Query(InnerJoinSelectClause(XRefFromColumn, toTable, throughTable, XRefToColumn, ToTableColumn, PropertyContainingIdValue, model)) as IEnumerable<dynamic>).ToList();

                foreach (var m in models) AddReferenceBackToModel(m, model);

                EagerLoadMany.Cache = new DynamicModels(models);

                AddNewAssociationMethod(EagerLoadMany.Cache, model);

                return EagerLoadMany.Cache;
            };
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            string sql = InnerJoinSelectClause(XRefFromColumn, toTable, throughTable, XRefToColumn, ToTableColumn, PropertyContainingIdValue, models.ToArray());

            return EagerLoadMany.Execute(options, Repository, MethodName, sql, models, XRefFromColumn);
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

        DynamicRepository reference;

        string toTable;

        public string XRefTable { get; set; }

        public string XRefToColumn { get; set; }

        public string XRefFromColumn { get; set; }

        public string ToTableColumn { get; set; }

        public string PropertyContainingIdValue { get; set; }

        public HasManyAndBelongsTo(DynamicRepository repository, DynamicRepository reference)
        {
            Repository = repository;

            this.reference = reference;

            var sorted = new[] { repository.TableName, reference.TableName }.OrderBy(s => s);

            throughTable = sorted.First() + sorted.Last();

            MethodName = repository.TableName;
        }

        public void Init(dynamic model)
        {
            EagerLoadMany = new EagerLoadMany();

            throughTable = XRefTable ?? throughTable;

            toTable = Repository.TableName;

            XRefFromColumn = XRefFromColumn ?? SigularId(model);

            XRefToColumn = XRefToColumn ?? SigularId(Repository);

            ToTableColumn = ToTableColumn ?? Id();

            PropertyContainingIdValue = PropertyContainingIdValue ?? Id();

            AddAssociationMethods(model);

            Model = model;
        }

        public void AddAssociationMethods(dynamic model)
        {
            model.SetMember(
                MethodName,
                InnerJoinFor(model));

            model.SetMember(
                Singular(MethodName) + "Ids",
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

                string innerJoinSelectClause = InnerJoinSelectClause(XRefFromColumn, toTable, throughTable, XRefToColumn, ToTableColumn, PropertyContainingIdValue, model);

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
            var sql = InnerJoinSelectClause(XRefFromColumn, toTable, throughTable, XRefToColumn, ToTableColumn, PropertyContainingIdValue, models.ToArray());

            return EagerLoadMany.Execute(options, Repository, MethodName, sql, models, XRefFromColumn);
        }
    }

    public class SingleAssociation : Association
    {

    }

    public class EagerLoadSingleForAll
    {
        public static DynamicModels Execute(IEnumerable<dynamic> models,
            DynamicRepository repository,
            string associationName,
            string sql,
            Func<dynamic, dynamic, bool> findClause)
        {
            var belongsResult = new List<dynamic>(repository.Query(sql));

            foreach (var item in belongsResult)
            {
                var relatedModels = models.Where(s => findClause(item, s));

                foreach (var relateModel in relatedModels)
                {
                    var association = relateModel.AssociationNamed(associationName);

                    association.Model = item;

                    item.SetMember(relateModel.GetType().Name, relateModel);
                }
            }

            return new DynamicModels(belongsResult);
        }
    }

    public class HasOne : SingleAssociation
    {
        public string ForeignKey { get; set; }

        public HasOne(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public HasOne(DynamicRepository repository, string methodName)
        {
            this.Repository = repository;

            MethodName = methodName ?? Singular(Repository);
        }

        public void Init(dynamic model)
        {
            model.SetMember(
                MethodName,
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public string ForeignKeyName(dynamic model)
        {
            return string.IsNullOrEmpty(ForeignKey) ? SigularId(model) : ForeignKey;
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var foreignKeyName = ForeignKeyName(models.First()) as string;

            var ones = @"
            select * from {fromTable} 
            where {foreignKey}
            in ({inClause})"
                .Replace("{fromTable}", Repository.TableName)
                .Replace("{foreignKey}", foreignKeyName)
                .Replace("{inClause}", InClause(models, Id()));

            return EagerLoadSingleForAll.Execute(models,
                       Repository,
                       MethodName,
                       ones,
                       (result, source) => source.Id == result.GetMember(foreignKeyName));
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            Model = Repository.SingleWhere(ForeignKeyName(model) + " = @0", model.GetMember(Id()));

            return Model;
        }
    }

    public class HasOneThrough : SingleAssociation
    {
        private DynamicRepository through;

        public string XRefToColumn { get; set; }

        public string XRefFromColumn { get; set; }

        public string ToTableColumn { get; set; }

        public string PropertyContainingIdValue { get; set; }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through)
            : this(repository, through, null)
        {
        }

        public HasOneThrough(DynamicRepository repository, DynamicRepository through, string methodName)
        {
            this.Repository = repository;
            this.through = through;
            MethodName = methodName ?? Singular(Repository);
        }

        public void Init(dynamic model)
        {
            XRefFromColumn = XRefFromColumn ?? SigularId(model);

            XRefToColumn = XRefToColumn ?? SigularId(Repository);

            ToTableColumn = ToTableColumn ?? Id();

            PropertyContainingIdValue = PropertyContainingIdValue ?? Id();

            model.SetMember(
                Singular(Repository),
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            Model = Query(XRefFromColumn,
                Repository.TableName,
                through.TableName,
                XRefToColumn,
                ToTableColumn,
                PropertyContainingIdValue,
                new List<dynamic>() { model })();

            return Model;
        }

        private DynamicFunction Query(string xRefFromColumn, 
            string toTable, 
            string xRefTable, 
            string xRefToColumn, 
            string toTableColumn, 
            string propertyContainingIdValue, 
            List<dynamic> models)
        {
            return () => Repository.Query(InnerJoinSelectClause(xRefFromColumn, 
                                              toTable, 
                                              xRefTable, 
                                              xRefToColumn, 
                                              toTableColumn, 
                                              propertyContainingIdValue, 
                                              models.ToArray())).FirstOrDefault();
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var xRefFromColumn = XRefFromColumn;

            var sql = InnerJoinSelectClause(xRefFromColumn,
                Repository.TableName,
                through.TableName,
                XRefToColumn,
                ToTableColumn,
                PropertyContainingIdValue, models.ToArray());

            return EagerLoadSingleForAll.Execute(models,
                       Repository,
                       MethodName,
                       sql,
                       (result, source) => source.Id == result.GetMember(xRefFromColumn));
        }
    }

    public class BelongsTo : SingleAssociation
    {
        public string PropertyContainingIdValue { get; set; }

        public string IdColumnOfParentTable { get; set; }

        public BelongsTo(DynamicRepository repository)
            : this(repository, null)
        {

        }

        public BelongsTo(DynamicRepository repository, string methodName)
        {
            this.Repository = repository;

            MethodName = methodName ?? Singular(repository);
        }

        public void Init(dynamic model)
        {
            PropertyContainingIdValue = string.IsNullOrEmpty(PropertyContainingIdValue) ? SigularId(Repository) : PropertyContainingIdValue;

            IdColumnOfParentTable = string.IsNullOrEmpty(IdColumnOfParentTable) ? "Id" : IdColumnOfParentTable;

            model.SetMember(
                MethodName,
                new DynamicFunctionWithParam((options) => GetModelOrCache(model, options)));
        }

        public IEnumerable<dynamic> EagerLoad(IEnumerable<dynamic> models, dynamic options)
        {
            var ones = @"
            select * from {fromTable} 
            where {primaryKey}
            in ({inClause})"
                .Replace("{fromTable}", Repository.TableName)
                .Replace("{primaryKey}", IdColumnOfParentTable)
                .Replace("{inClause}", InClause(models, PropertyContainingIdValue));

            return EagerLoadSingleForAll.Execute(models, 
                       Repository, 
                       MethodName, 
                       ones, 
                       (result, source) => result.GetMember(IdColumnOfParentTable) == source.GetMember(PropertyContainingIdValue));
        }

        public dynamic GetModelOrCache(dynamic model, dynamic options)
        {
            if (DiscardCache(options)) Model = null;

            if (Model != null) return Model;

            string whereClause = string.Format("{0} = @0", IdColumnOfParentTable);

            Model = Repository.SingleWhere(whereClause, model.GetMember(PropertyContainingIdValue));

            return Model;
        }
    }
}
