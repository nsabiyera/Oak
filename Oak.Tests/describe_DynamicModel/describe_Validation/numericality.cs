using NSpec;
using Oak.Tests.describe_DynamicModel.describe_Validation.Classes;

namespace Oak.Tests.describe_DynamicModel.describe_Validation
{
    class numericality : nspec
    {
        dynamic player;

        bool isValid;

        void before_each()
        {
            player = new Player();
            player.AveragePoints = 3.6;
            player.Age = 22;
            player.HeightInInches = 65;
            player.WeightInPounds = 185;
            player.NumberOfFingers = 10;
            player.LuckyEvenNumber = 36;
            player.LuckyOddNumber = 13;
        }

        void validating_numericality_of()
        {
            act = () => isValid = player.IsValid();

            context["average points is not a number"] = () =>
            {
                before = () => player.AveragePoints = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["average points is a number"] = () =>
            {
                before = () => player.AveragePoints = 3.6;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["age is not a number"] = () =>
            {
                before = () => player.Age = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["age is not an integer"] = () =>
            {
                before = () => player.Age = 22.1;

                it["is not valid"] = () => isValid.should_be_false();
            };
            
            context["age is an integer"] = () =>
            {
                before = () => player.Age = 22;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["height in inches is not a number"] = () =>
            {
                before = () => player.HeightInInches = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["height in inches is a number"] = () =>
            {
                before = () => player.HeightInInches = 65;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["height in inches is a number but less than lower threshold"] = () =>
            {
                before = () => player.HeightInInches = 59.9;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["height in inches is a number but equal to lower threshold"] = () =>
            {
                before = () => player.HeightInInches = 60;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["height in inches is a number and is greather than lower threshold"] = () =>
            {
                before = () => player.HeightInInches = 60.1;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["weight in pounds is not a number"] = () =>
            {
                before = () => player.WeightInPounds = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["weight in pounds is a number but less than threshold"] = () =>
            {
                before = () => player.WeightInPounds = 184.9;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["weight in pounds is a number and is equal to threshold"] = () =>
            {
                before = () => player.WeightInPounds = 185;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["weight in pounds is a number and is greater than threshold"] = () =>
            {
                before = () => player.WeightInPounds = 185.1;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["number of fingers is not a number"] = () =>
            {
                before = () => player.NumberOfFingers = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["number of fingers is a number but less than threshold"] = () =>
            {
                before = () => player.NumberOfFingers = 9.9;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["number of fingers is a number but greater than threshold"] = () =>
            {
                before = () => player.NumberOfFingers = 10.1;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["number of fingers is a number and equal to threshold"] = () =>
            {
                before = () => player.NumberOfFingers = 10;

                it["is valid"] = () => isValid.should_be_true();
            };
            
            context["height in inches is a number but greater than upper threshold"] = () =>
            {
                before = () => player.HeightInInches = 72.1;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["height in inches is a number but equal to upper threshold"] = () =>
            {
                before = () => player.HeightInInches = 72;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["height in inches is a number and less than upper threshold"] = () =>
            {
                before = () => player.HeightInInches = 71.9;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["weight in pounds is a number but greater than upper threshold"] = () =>
            {
                before = () => player.WeightInPounds = 300.1;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["weight in pounds is a number and equal to upper threshold"] = () =>
            {
                before = () => player.WeightInPounds = 300;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["weight in pounds is a number and less than upper threshold"] = () =>
            {
                before = () => player.WeightInPounds = 299.9;

                it["is valid"] = () => isValid.should_be_true();
            };
            
            context["lucky even number is not a number"] = () =>
            {
                before = () => player.LuckyEvenNumber = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["lucky even number is a number but is not even"] = () =>
            {
                before = () => player.LuckyEvenNumber = 7;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["lucky even number is a number and is even"] = () =>
            {
                before = () => player.LuckyEvenNumber = 36;

                it["is valid"] = () => isValid.should_be_true();
            };

            context["lucky odd number is not a number"] = () =>
            {
                before = () => player.LuckyOddNumber = "not a number";

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["lucky odd number is a number but is not odd"] = () =>
            {
                before = () => player.LuckyOddNumber = 10;

                it["is not valid"] = () => isValid.should_be_false();
            };

            context["lucky odd number is a number and is odd"] = () =>
            {
                before = () => player.LuckyOddNumber = 13;

                it["is valid"] = () => isValid.should_be_true();
            };
        }
    }
}
