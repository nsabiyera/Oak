using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Author : DynamicModel
    {
        Profiles profiles = new Profiles();

        public Author(object dto)
            : base(dto)
        {

        }

        public IEnumerable<dynamic> Associates()
        {
            yield return new HasOne(profiles);
        }
    }
}
