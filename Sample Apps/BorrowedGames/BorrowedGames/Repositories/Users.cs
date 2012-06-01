using System;
using Massive;
using BorrowedGames.Models;

namespace BorrowedGames.Repositories
{
    public class Users : DynamicRepository
    {
        public Users()
        {
            Projection = d => new User(d);
        }

        public dynamic ForHandle(string handle)
        {
            return SingleWhere("Handle = @0", handle);
        }

        public dynamic ForEmail(string email)
        {
            return SingleWhere("Email = @0", email);
        }
    }
}