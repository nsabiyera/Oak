using System;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Users : DynamicRepository
    {
        public Users()
        {
            Projection = d => new User(d);
        }
    }
}
