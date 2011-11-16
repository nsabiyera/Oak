using System;
using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Email : DynamicModel
    {
        Aliases aliases = new Aliases();

        public Email(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(aliases);
        }
    }
}
