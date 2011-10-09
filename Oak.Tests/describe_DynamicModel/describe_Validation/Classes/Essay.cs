using System.Collections.Generic;
using System.Linq;

namespace Oak.Tests.describe_DynamicModel.describe_Validation.Classes
{
    public class Essay : DynamicModel
    {
        public Essay()
            : this(new { })
        {
        }

        public Essay(dynamic dto)
        {
            Init(dto);
        }

        public IEnumerable<dynamic> Validates()
        {
            yield return new Length("Title") {Minimum = 1};
            yield return new Length("Author") {Maximum = 5};
            yield return new Length("Publisher") {In = Enumerable.Range(1,10)};
            yield return new Length("Version") {Is = 1};
            yield return new Length("Year") {Is = 4};
        }
    }
}
