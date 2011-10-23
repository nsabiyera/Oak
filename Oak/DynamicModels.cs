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