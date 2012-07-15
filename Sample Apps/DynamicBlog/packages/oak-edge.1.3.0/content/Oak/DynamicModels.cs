using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Dynamic;

namespace Oak
{
    public static class DynamicModelExtensions
    {
        public static DynamicModels ToModels(this IEnumerable<dynamic> enumerable)
        {
            return new DynamicModels(enumerable);
        }
    }

    public class DynamicModels : Gemini, IEnumerable<object>
    {
        public List<dynamic> Models { get; set; }

        public DynamicModels(IEnumerable<dynamic> models)
        {
            Models = models.ToList();
        }

        public new IEnumerable<dynamic> Select(params string[] properties)
        {
            foreach (dynamic model in Models) yield return Select(model, properties);
        }

        dynamic Select(dynamic model, params string[] properties)
        {
            var hash = (model as object).ToDictionary();

            var expando = new ExpandoObject() as IDictionary<string, object>;

            hash.Where(s => properties.Contains(s.Key)).ForEach(kvp => expando.Add(kvp.Key, kvp.Value));

            if (expando.Count == 1) return expando.First().Value;

            return expando;
        }
               
        public bool Any(dynamic options)
        {
            options = (options as object).ToExpando();

            foreach (dynamic model in Models) if (IsMatch(options, model)) return true;

            return false;
        }

        public List<dynamic> ToList()
        {
            return new List<dynamic>(Models);
        }

        public bool Any()
        {
            return Models.Any();
        }

        public int Count()
        {
            return Models.Count;
        }

        public dynamic First()
        {
            return Models.FirstOrDefault();
        }

        public dynamic Last()
        {
            return Models.LastOrDefault();
        }

        public dynamic Last(dynamic options)
        {
            return Where(options as object).LastOrDefault();
        }

        public dynamic First(dynamic options)
        {
            return Where(options as object).FirstOrDefault();
        }

        public dynamic OrderBy(dynamic options)
        {
            var dict = (options as object).ToExpando() as IDictionary<string, object>;

            dynamic results = Models.AsEnumerable();

            dict.ForEach(kvp => results = Sort(results, kvp.Key, kvp.Value));

            return new DynamicModels(results);
        }

        public dynamic Sort(IEnumerable<dynamic> models, string property, object direction)
        {
            if (models is IOrderedEnumerable<dynamic>)
            {
                var ordered = (models as IOrderedEnumerable<dynamic>);

                if (IsAscending(direction)) return ordered.ThenBy(s => ValueFor(s, property));

                return ordered.ThenByDescending(s => ValueFor(s, property));
            }

            if (IsAscending(direction)) return models.OrderBy(s => ValueFor(s, property));

            return models.OrderByDescending(s => ValueFor(s, property));
        }

        public dynamic ValueFor(dynamic model, string property)
        {
            return ValueFor((ToHash(model) as IDictionary<string, object>)[property]);
        }

        public bool IsAscending(object value)
        {
            return (value as string) == "asc";
        }

        public DynamicModels Where(dynamic options)
        {
            options = (options as object).ToExpando();

            var results = new List<dynamic>();

            foreach (dynamic model in Models) if (IsMatch(options, model)) results.Add(model);

            return new DynamicModels(results);
        }

        private bool IsMatch(IDictionary<string, dynamic> options, dynamic model)
        {
            IDictionary<string, object> hash = ToHash(model);

            return options.All(s => s.Value == ValueFor(hash[s.Key]));
        }

        private IDictionary<string, object> ToHash(dynamic model)
        {
            if (model is Gemini) return model.Hash();

            return (model as object).ToExpando();
        }

        private dynamic ValueFor(dynamic value)
        {
            if (value is Delegate) return value();

            return value;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (RespondsTo(binder.Name)) return base.TryInvokeMember(binder, args, out result);

            return SelectMany(binder.Name, args, out result);
        }

        private bool SelectMany(string collectionName, object[] args, out object result)
        {
            result = new List<dynamic>();

            if (!Models.Any()) return true;

            dynamic options = null;

            if(args.Any()) options = args[0];

            var association = Models[0].AssociationNamed(collectionName);

            result = association.SelectManyRelatedTo(Models, options);

            return true;
        }

        private dynamic Get(string collectionName, dynamic model)
        {
            return model.GetMember(collectionName).Invoke(null);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return Models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Models.GetEnumerator();
        }
    }
}
