using System;
using Massive;

namespace BorrowedGames.Models
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
    }
}