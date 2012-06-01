using System;
using System.Collections.Generic;
using Oak;
using BorrowedGames.Repositories;

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