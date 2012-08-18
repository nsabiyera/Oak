using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class format_for_class_with_auto_props : format
    {
        void before_each()
        {
            product = new ProductWithAutoProperties();
        }
    }

    class format_for_dynamic_class : format
    {
        void before_each()
        {
            product = new Product();
        }
    }

    abstract class format : nspec
    {
        public dynamic product;

        public bool isValid;

        void validating_format()
        {
            act = () => isValid = product.IsValid();

            context["product code does not match format of all characters"] = () =>
            {
                before = () =>
                {
                    product.Code = "1232123";
                    product.ProductId = 123456;
                };

                it["is invalid"] = () => isValid.should_be_false();
            };

            context["product code matches format of all characters"] = () =>
            {
                before = () =>
                {
                    product.Code = "ABD";
                    product.ProductId = 123456;
                };

                it["is valid"] = () => isValid.should_be_true();
            };

            context["product id does not match regex"] = () =>
            {
                before = () =>
                {
                    product.Code = "ABD";
                    product.ProductId = 1231;
                };

                it["is invalid"] = () =>
                {   
                    product.IsValid();
                    (product.Errors()[0].Value as string).should_be("ProductId is invalid.");
                    isValid.should_be_false();
                };
            };

            context["product code is null"] = () =>
            {
                before = () =>
                {
                    product.Code = default(string);
                    product.ProductId = null;
                };

                it["is invalid"] = () => isValid.should_be_false();
            };
        }
    }
}
