using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Massive;
using Oak;

namespace BorrowedGames.Repositories
{
    public class Games : DynamicRepository
    {
        public IEnumerable<dynamic> StartsWith(string name)
        {
            return Like(name + "%");
        }

        public IEnumerable<dynamic> Contains(string name)
        {
            return Like(AddWildCard(name));
        }

        public IEnumerable<dynamic> HasWords(string words)
        {
            return Like(
                words.Split(' ')
                     .Select(AddWildCard)
                     .Aggregate((i, j) => i + j));
        }

        public IEnumerable<dynamic> Like(string name)
        {
            return All("Name like @0", args: new[] { name });
        }

        private string AddWildCard(string s)
        {
            return "%" + s + "%";
        }
    }
}
