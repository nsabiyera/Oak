using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Massive;
using BattleShip.Models;

namespace BattleShip.Repositories
{
    public class Games : DynamicRepository
    {
        public Games()
        {
            Projection = d => new Game(d);
        }
    }
}