using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicModels
{
    class select_properties_from_collection : _dynamic_models
    {
        void it_creates_a_projection_for_empty_list()
        {
            models = new DynamicModels(new List<dynamic>());

            var result = models.Select("Name") as IEnumerable<dynamic>;

            result.Count().should_be(0);
        }

        void it_creates_a_projection_for_populated_list()
        {
            models = new DynamicModels(new List<dynamic>() 
            { 
                new DynamicModel().Init(new { Name = "Jane" }),
                new { Name = "Jane" },
                new Gemini(new { Name = "Jane" })
            });

            var result = (models.Select("Name") as IEnumerable<dynamic>).ToList();

            (result[0].Name as string).should_be("Jane");

            (result[1].Name as string).should_be("Jane");

            (result[2].Name as string).should_be("Jane");
        }
    }
}
