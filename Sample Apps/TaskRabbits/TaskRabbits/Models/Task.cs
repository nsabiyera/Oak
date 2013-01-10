using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using TaskRabbits.Repositories;

namespace TaskRabbits.Models
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