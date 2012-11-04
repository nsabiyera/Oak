using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;

namespace Oak.Tests.describe_DynamicModel
{
    class describe_StrongParamerters : nspec
    {
        void specify_permitted_parameters_are_allowed_to_be_set_view_mass_assignment()
        {
            dynamic foobar = new Gemini(new { FirstName = "Jane", LastName = "Doe" });

            foobar.Extend<StrongParameters>();

            foobar.Permit("FirstName", "LastName");

            foobar.SetMembers(new { FirstName = "First", LastName = "Last" });

            (foobar.FirstName as string).should_be("First");

            (foobar.LastName as string).should_be("Last");
        }

        void specify_exception_is_thrown_if_mass_assignment_is_performed_on_non_permitted_parameters()
        {
            dynamic foobar = new Gemini(new { FirstName = "Jane", LastName = "Doe" });

            foobar.Extend<StrongParameters>();

            foobar.Permit("FirstName");

            try
            {
                foobar.SetMembers(new { FirstName = "First", LastName = "Last" });

                throw new Exception("Exception was not thrown");
            }
            catch (InvalidOperationException)
            {
                
            }
        }
    }
}
