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