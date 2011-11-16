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
               
        public bool Any(dynamic options)
        {
            options = (options as object).ToExpando();

            foreach (dynamic model in Models) if (IsMatch(options, model)) return true;

            return false;
        }

        public dynamic First()
        {
            return Models.FirstOrDefault();
        }

        public dynamic First(dynamic options)
        {
            return Where(options as object).FirstOrDefault();
        }

        public IEnumerable<dynamic> Where(dynamic options)
        {
            options = (options as object).ToExpando();

            foreach (dynamic model in Models) if (IsMatch(options, model)) yield return model;
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

            return SelectMany(binder.Name, out result);
        }

        private bool SelectMany(string collectionName, out object result)
        {
            result = new List<dynamic>();

            if (Models.Count == 0) return true;

            var association = Models[0].AssociationNamed(collectionName);

            result = association.SelectManyRelatedTo(Models);

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