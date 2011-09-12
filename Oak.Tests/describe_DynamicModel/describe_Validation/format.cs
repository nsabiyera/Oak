using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NSpec;
using Oak.Tests.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class format : nspec
    {
        dynamic product;

        bool isValid;

        void before_each()
        {
            product = new Product();
        }

        void validating_format()
        {
            act = () => isValid = product.IsValid();

            context["product code does not match format of all characters"] = () =>
            {
                before = () => product.Code = "1232123";

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["product code matches format of all characters"] = () =>
            {
                before = () => product.Code = "ABD";

                it["is valid"] = () => isValid.should_be_true();
            };

            context["product code is null"] = () =>
            {
                before = () => product.Code = default(string);

                it["is invalid"] = () => isValid.should_be_false();
            };
        }
    }
}
