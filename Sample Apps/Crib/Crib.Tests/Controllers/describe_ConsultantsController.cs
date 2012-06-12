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

        void updating_consultant()
        {
            before = () =>
                consultantId = new { Name = "" }.InsertInto("Consultants");

            act = () => controller.Update(new
            {
                Id = consultantId,
                Name = "Person 1",
                RollOffDate = DateTime.Today
            });

            it["works"] = () =>
            {
                var consultant = consultants.All().First();

                (consultant.Name as string).should_be("Person 1");

                ((DateTime)consultant.RollOffDate).should_be(DateTime.Today);
            };
        }
    }
}
