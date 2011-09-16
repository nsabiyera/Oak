using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Users : DynamicRepository
    {
        public Users()
        {
            Projection = d => new User(d);
        }
    }
}