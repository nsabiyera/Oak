using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Oak;
using TaskRabbits.Repositories;

namespace TaskRabbits.Models
{
    public class Rabbit : DynamicModel
    {
        Rabbits rabbits = new Rabbits();

        public Rabbit(object dto)
            : base(dto)
        {
            
        }

        IEnumerable<dynamic> Validates()
        {
            yield return new Presence("Name");

            yield return new Uniqueness("Name", rabbits);
        }

        void Save()
        {
            if (!RespondsTo("Id")) _.Id = rabbits.Insert(this);

            else rabbits.Save(this);
        }
    }
}