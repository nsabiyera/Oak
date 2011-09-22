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
        IEnumerable<dynamic> models;

        public DynamicModels(IEnumerable<dynamic> models)
        {
            this.models = models;
        }

        public IEnumerator<object> GetEnumerator()
        {
            return models.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return models.GetEnumerator();
        }
    }
}