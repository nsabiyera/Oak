using System.Collections.Generic;

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
        }
    }
}
