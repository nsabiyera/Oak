using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;

namespace Oak
{
    public class Bullet
    {
        static string redundantQueryErrorMessage =
@"This query is redundant, consider cacheing the value returned.  
This may be a possible N+1 issue, try eager loading...if you 
are using DynamicRepository, use the Include() method, for example: 
comments.All().Include(""Blog"").";

        static string nPlusOneQueryErrorMessage =
@"Possible N+1, try eager loading...if you are using 
DynamicRepository, use the Include() method, for example: 
blogs.All().Include(""Comments"", ""Tags"").";

        public static IEnumerable<dynamic> InefficientQueries(List<string> sql)
        {
            var inefficientQueries = new List<dynamic>();

            EachConsecutive2(sql, (first, second) =>
            {
                if (IsSqlExact(first, second))
                {
                    if (inefficientQueries.Any(s => IsSqlExact(first, s.Query))) return;

                    inefficientQueries.Add(new Gemini(new
                    {
                        Query = first,
                        Reason = redundantQueryErrorMessage
                    }));
                }
                else if (IsSqlSimilar(first, second))
                {
                    inefficientQueries.Add(new Gemini(new
                    {
                        Query = first,
                        Reason = nPlusOneQueryErrorMessage
                    }));

                    inefficientQueries.Add(new Gemini(new
                    {
                        Query = second,
                        Reason = nPlusOneQueryErrorMessage
                    }));
                }
            });

            return new DynamicModels(inefficientQueries);
        }

        public static bool IsSqlSimilar(string first, string second)
        {
            return SqlExcludingInStatement(first) == SqlExcludingInStatement(second);
        }

        private static bool IsSqlExact(string first, string second)
        {
            return first == second;
        }

        public static string SqlExcludingInStatement(string sql)
        {
            return Regex.Replace(sql, @" in \([^)]*\)", "");
        }

        public static IEnumerable<T> EachConsecutive2<T>(IEnumerable<T> source, Action<T, T> action)
        {
            var array = source.ToArray();
            for (int i = 0; i < array.Length - 1; i++)
            {
                action(array[i], array[i + 1]);
            }

            return source;
        }
    }
}