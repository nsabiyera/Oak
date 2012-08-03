using System.Collections.Generic;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class PlayerWithAutoProps : DynamicModel
    {
        public PlayerWithAutoProps()
            : this(new { })
        {
        }

        public PlayerWithAutoProps(object dto)
            : base(dto)
        {
        }

        public double AveragePoints { get; set; }
        public double Age { get; set; }
        public double HeightInInches { get; set; }
        public double WeightInPounds { get; set; }
        public double NumberOfFingers { get; set; }
        public double LuckyEvenNumber { get; set; }
        public double LuckyOddNumber { get; set; }

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
