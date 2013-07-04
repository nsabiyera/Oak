using System;
using System.Collections.Generic;
using System.Linq;
using Massive;
using TaskRabbits.Models;

namespace TaskRabbits.Repositories
{
    public class Tasks : DynamicRepository
    {
        public Tasks()
        {
            Projection = d => new Task(d);
        }

        dynamic ForRabbit(dynamic rabbitId)
        {
            return All(where: "RabbitId = @0", args: new object[] { rabbitId });
        }
    }
}
