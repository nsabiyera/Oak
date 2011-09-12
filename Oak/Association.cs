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
    }

    public class HasMany : Association
    {
        public DynamicRepository Repository { get; set; }

        public string MethodToCreate { get; set; }

        public string ReferencedBy { get; set; }

        public HasMany(DynamicRepository repository)
        {
            Repository = repository;
            MethodToCreate = repository.GetType().Name;
            ReferencedBy = null;
        }

        public override void Init(DynamicModel model)
        {
            (model.Virtual as Prototype).SetValueFor(
                MethodToCreate,
                new Func<IEnumerable<dynamic>>(
                    () => Repository.All((ReferencedBy ?? (model.GetType().Name + "Id")) + " = @0", args: new[] { model.Expando.Id })));
        }
    }

    public class BelongsTo : Association
    {
        public DynamicRepository Repository { get; set; }

        public string Name { get; set; }

        public override void Init(DynamicModel model)
        {
            (model.Virtual as Prototype).SetValueFor(
                Name,
                new Func<dynamic>(() => null));
        }

    }
}