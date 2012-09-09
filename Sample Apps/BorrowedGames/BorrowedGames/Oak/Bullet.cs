using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

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

        public static IEnumerable<dynamic> InefficientQueries(List<Tuple<string, string, int>> sqlAndStackTrace)
        {
            var inefficientQueries = new List<dynamic>();

            var orderedSql = new List<dynamic>();

            for (int i = 0; i < sqlAndStackTrace.Count; i++)
            {
                var record = sqlAndStackTrace[i];

                orderedSql.Add(new Gemini(new
                {
                    Order = i,
                    Query = record.Item1,
                    StackTrace = record.Item2,
                    ThreadId = record.Item3
                }));
            }

            var grouped = orderedSql.GroupBy(s => s.ThreadId);

            foreach(var group in grouped)
            {
                EachConsecutive2(group.OrderBy(s => s.Order), (first, second) =>
                {
                    if (IsExactStringMatch(first.Query, second.Query))
                    {
                        inefficientQueries.Add(new Gemini(new
                        {
                            Query = first.Query,
                            Reason = redundantQueryErrorMessage,
                            StackTrace = ScrubStackTrace(first.StackTrace),
                            ThreadId = first.ThreadId
                        }));

                        inefficientQueries.Add(new Gemini(new
                        {
                            Query = second.Query,
                            Reason = redundantQueryErrorMessage,
                            StackTrace = ScrubStackTrace(second.StackTrace),
                            ThreadId = second.ThreadId
                        }));
                    }
                    else if (IsSqlSimilar(first.Query, second.Query))
                    {
                        inefficientQueries.Add(new Gemini(new
                        {
                            Query = first.Query,
                            Reason = nPlusOneQueryErrorMessage,
                            StackTrace = ScrubStackTrace(first.StackTrace),
                            ThreadId = first.ThreadId
                        }));

                        inefficientQueries.Add(new Gemini(new
                        {
                            Query = second.Query,
                            Reason = nPlusOneQueryErrorMessage,
                            StackTrace = ScrubStackTrace(second.StackTrace),
                            ThreadId = second.ThreadId
                        }));
                    }
                });    
            }

            return new DynamicModels(inefficientQueries);
        }

        public static bool IsSqlSimilar(string first, string second)
        {
            return SqlExcludingInStatement(first) == SqlExcludingInStatement(second);
        }

        private static bool IsExactStringMatch(string first, string second)
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

        public static string ScrubStackTrace(string stackTrace)
        {
            var sb = new StringBuilder();

            using (StringReader sr = new StringReader(stackTrace))
            {
                var line = null as string;

                while ((line = sr.ReadLine()) != null)
                {
                    if (new List<string>
                    {
                        "at System.",
                        "at Oak.",
                        "at lambda_method",
                        "at Massive.",
                        "at Castle.",
                        "at CallSite.",
                        "at Glimpse."
                    }.Any(s => line.Contains(s))) continue;

                    sb.AppendLine(line);
                }
            }

            return sb.ToString();
        }
    }
}