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

        public string Name { get; set; }

        public string Relation { get; set; }

        public HasMany(DynamicRepository repository)
        {
            Repository = repository;
            Name = repository.GetType().Name;
            Relation = null;
        }

        public override void Init(DynamicModel model)
        {
            (model.Virtual as Prototype).SetValueFor(
                Name,
                new Func<IEnumerable<dynamic>>(
                    () => Repository.All(Relation ?? model.GetType().Name + "Id = @0", args: new[] { model.Expando.Id })));
        }
    }
}