using System;
using System.Collections.Generic;
using Oak;
using System.Text;
using System.Security.Cryptography;
using BorrowedGames.Repositories;

namespace BorrowedGames.Models
{
    public class Registration : DynamicModel
    {
        Users users = new Users();

        public Registration()
            : this(new { })
        {
        }

        public Registration(object entity)
            : base(entity)
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return
            new Presence("Email");

            yield return
            new Format("Email")
            {
                With = @"^(([A-Za-z0-9]+_+)|([A-Za-z0-9]+\-+)|([A-Za-z0-9]+\.+)|([A-Za-z0-9]+\++))*[A-Za-z0-9]+@((\w+\-+)|(\w+\.))*\w{1,63}\.[a-zA-Z]{2,6}$"
            };

            yield return
            new Uniqueness("Email", users) { ErrorMessage = "Email is unavailable." };

            yield return
            new Presence("Password") { ErrorMessage = _.IDoNotExist };

            yield return
            new Confirmation("Password") { ErrorMessage = "Passwords do not match." };
        }

        public void Register()
        {
            _.Password = Encrypt(_.Password);

            users.Insert(this.Exclude("PasswordConfirmation"));
        }

        public static string Encrypt(string password)
        {
            var hash = Encoding.Unicode.GetBytes(password);
            var md5 = new MD5CryptoServiceProvider();
            var md5hash = md5.ComputeHash(hash);
            return Convert.ToBase64String(hash);
        }
    }
}