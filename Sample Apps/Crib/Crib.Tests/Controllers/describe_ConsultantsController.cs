using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak;
using Crib.Controllers;
using Crib.Repositories;

namespace Crib.Tests.Controllers
{
    class describe_ConsultantsController : describe_Crib
    {
        dynamic consultantId;

        ConsultantsController controller;

        Consultants consultants;

        void before_each()
        {
            controller = new ConsultantsController();

            consultants = new Consultants();
        }

        void specify__updating_consultant()
        {
            consultantId = new { Name = "" }.InsertInto("Consultants");

            controller.Update(new
            {
                Id = consultantId,
                Name = "Person 1",
                RollOffDate = DateTime.Today,
                Picture = "http://temp.com/img"
            });

            var consultant = consultants.All().First();

            (consultant.Name as string).should_be("Person 1");

            ((DateTime)consultant.RollOffDate).should_be(DateTime.Today);

            (consultant.Picture as string).should_be("http://temp.com/img");
        }

        void specify_creating_consultant()
        {
            controller.Create(new
            {
                Name = "Person 2",
                RollOffDate = Tomorrow()
            });

            var consultant = consultants.All().First();

            (consultant.Name as string).should_be("Person 2");

            ((DateTime)consultant.RollOffDate).should_be(Tomorrow());
        }
    }
}
