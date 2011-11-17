using System;
using Oak;
using System.Linq;
using System.Collections.Generic;

namespace BorrowedGames.Models
{
    public static class ObjectExtensions
    {
        public static IEnumerable<TSource> Distinct<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (TSource element in source) if (seenKeys.Add(keySelector(element))) yield return element;
        }

        public static IEnumerable<TSource> LeftJoin<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            return source.Where(predicate).DefaultIfEmpty();
        }

        public static bool DoesntContain<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return !source.Contains(value);
        }
    }
}
