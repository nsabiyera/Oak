using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Oak.Models;

namespace Oak.Tests.describe_DynamicModel.SampleClasses
{
    public class Registration : DynamicModel
    {
        public Registration()
        {
            Validates("Email", Presense);
            Validates("Password", Presense);
            Validates("Password", Confirmation);
            Validates("PasswordConfirmation", Presense);
        }
    }
}
