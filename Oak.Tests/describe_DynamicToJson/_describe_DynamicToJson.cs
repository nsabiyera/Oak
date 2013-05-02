using System;
using System.Collections.Generic;
using System.Linq;
using NSpec;
using Oak.Tests.describe_DynamicModels.Classes;

namespace Oak.Tests
{
    class _describe_DynamicToJson : nspec
    {
        public dynamic objectToConvert;

        public string jsonString;

        void before_each()
        {
            jsonString = null;
        }
    }
}
