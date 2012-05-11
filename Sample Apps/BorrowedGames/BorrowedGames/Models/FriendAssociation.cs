using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using Oak;

namespace BorrowedGames.Models
{
    public class FriendAssociation : DynamicModel
    {
        Users users = new Users();

        public FriendAssociation(object dto) : base(dto) { }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(users);
        }
    }
}