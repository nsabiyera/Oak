using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

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

            Validates(new Uniqueness("Email") { Using = users });

            Init(dto);
        }
    }
}
