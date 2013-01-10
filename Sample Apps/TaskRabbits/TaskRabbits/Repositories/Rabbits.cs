using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using TaskRabbits.Models;

namespace TaskRabbits.Repositories
{
    public class Rabbits : DynamicRepository
    {
        public Rabbits()
        {
            Projection = d => new Rabbit(d);
        }
    }
}