using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Player : DynamicModel
    {
        public Player()
            : this(new { })
        {
        }

        public Player(object dto)
            : base(dto)
        {
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Numericality("AveragePoints");
            yield return new Numericality("Age") { OnlyInteger = true };
            yield return new Numericality("HeightInInches") { GreaterThan = 60, LessThan = 72 };
            yield return new Numericality("WeightInPounds") { GreaterThanOrEqualTo = 185, LessThanOrEqualTo = 300 };
            yield return new Numericality("NumberOfFingers") { EqualTo = 10 };
            yield return new Numericality("LuckyEvenNumber") { Even = true };
            yield return new Numericality("LuckyOddNumber") { Odd = true };
        }
    }
}
