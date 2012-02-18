using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class User : DynamicModel
    {
        Users users;

        public User()
            : this(new { })
        {
        }

        public User(object dto)
            : base(dto)
        {
            users = new Users();
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Uniqueness("Email", users);
        }
    }
}
