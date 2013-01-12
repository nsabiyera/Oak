using System;
using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModels.Classes
{
    public class Task : DynamicModel
    {
        Rabbits rabbits = new Rabbits();

        public Task(object dto)
            : base(dto)
        {

        }

        IEnumerable<dynamic> Associates()
        {
            yield return new BelongsTo(rabbits);
        }
    }
}
