using System;
using System.Collections.Generic;
using Massive;

namespace Oak.Tests.describe_DynamicModel.describe_Association.Classes
{
    public class Following : DynamicModel
    {
        Familys familys = new Familys();

        public Following(object dto)
            : base(dto)
        {
            
        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(familys);

            yield return new BelongsTo(familys, "Following") { PropertyContainingIdValue = "FollowingId" };
        }
    }

    public class Followings : DynamicRepository
    {
        public Followings()
        {
            Projection = d => new Following(d);
        }
    }
}
