using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;

namespace BorrowedGames.Models
{
    public class EmailHistorys : DynamicRepository
    {
        public bool Exists(int forUserId, DateTime date)
        {
            return All(
                   where: "UserId = @0 and CreatedAt = @1",
                   args: new object[] { forUserId, date }).Any();   
        }
    }
}