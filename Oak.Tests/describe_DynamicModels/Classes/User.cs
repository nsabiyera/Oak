using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class User : DynamicModel
    {
        Emails emails = new Emails();

        public User(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(emails);
        }
    }
}
