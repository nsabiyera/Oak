using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Crib.Repositories;

namespace Crib.Controllers
{
    public class ConsultantsController : Controller
    {
        Consultants consultants = new Consultants();

        public void Update(dynamic @params)
        {
            consultants.Update(@params, @params.Id);
        }

        public void Create(dynamic @params)
        {
            consultants.Insert(@params);
        }
    }
}
