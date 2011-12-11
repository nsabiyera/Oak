using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Dynamic;

namespace Oak
{
    public class DynamicModels : Gemini, IEnumerable<object>
    {
        public List<dynamic> Models { get; set; }

        public DynamicModels(IEnumerable<dynamic> models)
        {
            Models = models.ToList();
        }

        public IEnumerable<dynamic> Select(params string[] properties)
        {
            foreach (dynamic model in Models) yield return Select(model, properties);
        }

        dynamic Select(dynamic model, params string[] properties)
        {
            var hash = (model as object).ToDictionary();

            var expando = new ExpandoObject() as IDictionary<string, object>;

            hash.Where(s => properties.Contains(s.Key)).ForEach(kvp => expando.Add(kvp.Key, kvp.Value));

            if(expando.Count == 1) return expando.First().Value;

            return expando;
        }
               
        public bool Any(dynamic options)
        {
            options = (options as object).ToExpando();

            foreach (dynamic model in Models) if (IsMatch(options, model)) return true;

            return false;
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

        public DynamicModels Where(dynamic options)
        {
            options = (options as object).ToExpando();

            var results = new List<dynamic>();

            foreach (dynamic model in Models) if (IsMatch(options, model)) results.Add(model);

            return new DynamicModels(results);
        }

        private bool IsMatch(IDictionary<string, dynamic> options, dynamic model)
        {
            IDictionary<string, dynamic> hash = null;

            if (model is Gemini) hash = model.Hash();

            else hash = (model as object).ToExpando();

            return options.All(s => hash[s.Key] == s.Value);
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
