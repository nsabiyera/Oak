using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

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