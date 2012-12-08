using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;

namespace Oak.Tests.describe_DynamicRepository.Classes
{
    public class Records : DynamicRepository { }

    public class MemoizedRecords : DynamicRepository
    {
        public MemoizedRecords() : base("Records")
        {
            
        }

        static MemoizedRecords()
        {
            Gemini.Extend<MemoizedRecords, Memoize>();
        }

        IEnumerable<dynamic> Memoize()
        {
            yield return (DynamicFunctionWithParam)CurrentRecords;
        }

        dynamic CurrentRecords(dynamic startingWith)
        {
            return All(where: "Name like @0", args: new object[] { startingWith + "%" });
        }
    }
}