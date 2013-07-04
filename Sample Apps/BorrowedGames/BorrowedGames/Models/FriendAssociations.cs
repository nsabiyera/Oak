using System;
using Massive;

namespace BorrowedGames.Models
{
    public class FriendAssociations : DynamicRepository
    {
        public FriendAssociations()
        {
            Projection = d => new FriendAssociation(d);
        }
    }
}
