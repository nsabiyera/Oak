using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class EssayWithAutoProperties : DynamicModel
    {
        public EssayWithAutoProperties() : this(new { }) { }

        public EssayWithAutoProperties(object dto) : base(dto) { }

        public string Title { get; set; }
        public string Author { get; set; }
        public string Publisher { get; set; }
        public string Version { get; set; }
        public string ThisPropertyDoesNotExist { get; set; }
        public string IsNull { get; set; }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Length("Title") { Minimum = 1 };
            yield return new Length("Author") { Maximum = 5 };
            yield return new Length("Publisher") { In = Enumerable.Range(1, 10) };
            yield return new Length("Version") { Is = 1 };
            yield return new Length("ThisPropertyDoesNotExist") { IgnoreNull = true };
            yield return new Length("IsNull") { IgnoreNull = false };
        }
    }

    public class Essay : DynamicModel
    {
        public Essay() : this(new { }) { }

        public Essay(object dto) : base(dto) { }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Length("Title") { Minimum = 1 };
            yield return new Length("Author") { Maximum = 5 };
            yield return new Length("Publisher") { In = Enumerable.Range(1, 10) };
            yield return new Length("Version") { Is = 1 };
            yield return new Length("ThisPropertyDoesNotExist") { IgnoreNull = true };
            yield return new Length("IsNull") { IgnoreNull = false };
        }
    }
}
