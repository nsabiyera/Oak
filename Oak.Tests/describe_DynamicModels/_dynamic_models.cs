using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class _dynamic_models : nspec
    {
        public dynamic models;

        public bool resultForAny;

        public IEnumerable<dynamic> resultForWhere;

        public object resultForFirst;

        
    }
}
