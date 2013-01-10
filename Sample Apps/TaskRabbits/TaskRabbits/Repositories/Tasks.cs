using System;
using System.Collections.Generic;
using System.Linq;
using Massive;

namespace TaskRabbits.Repositories
{
    public class Tasks : DynamicRepository
    {
        dynamic ForRabbit(dynamic rabbitId)
        {
            return All(where: "RabbitId = @0", args: new object[] { rabbitId });
        }
    }
}
