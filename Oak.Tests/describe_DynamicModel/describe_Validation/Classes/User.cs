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

        public User(dynamic dto)
        {
            users = new Users();

            Init(dto);
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Uniqueness("Email", users);
        }
    }
}
