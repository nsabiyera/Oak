using System;
using Massive;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Library : DynamicRepository 
    {
        public override IDictionary<string, object> GetAttributesToSave(object o)
        {
            return base.GetAttributesToSave(o).Exclude("Id");
        }
    }
}
