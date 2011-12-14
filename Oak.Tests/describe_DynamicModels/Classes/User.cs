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

        Profiles profiles = new Profiles();

        public User(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasMany(emails);

            yield return new HasOne(profiles);
        }
    }
}