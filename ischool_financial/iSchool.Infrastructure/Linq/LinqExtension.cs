using System;
using System.Collections.Generic;
using System.Linq;

namespace iSchool
{
    public static partial class LinqExtension
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static bool In<T>(this T item, params T[] collection)
        {
            return collection.Contains(item);
        }

        public static T[] AsArray<T>(this IEnumerable<T> collection)
        {
            return collection is T[] arr ? arr : collection?.ToArray();
        }

        public static TV GetValueEx<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV defValue = default)
        {
            if (dict.TryGetValue(key, out var v)) return v;
            return defValue;
        }

        public static IDictionary<TK, TV> SetValueEx<TK, TV>(this IDictionary<TK, TV> dict, TK key, TV value)
        {
            dict[key] = value;
            return dict;
        }

        public static IDictionary<TK, TV> DelKeyEx<TK, TV>(this IDictionary<TK, TV> dict, TK key)
        {
            dict.Remove(key);
            return dict;
        }

        public static bool TryGetOne<T>(this IEnumerable<T> enumerable, Func<T, bool> condition, out T item) => TryGetOne(enumerable, out item, condition);
        public static bool TryGetOne<T>(this IEnumerable<T> enumerable, out T item, Func<T, bool> condition = null)
        {
            item = default;
            foreach (var item0 in enumerable)
            {
                if (condition == null || condition(item0))
                {
                    item = item0;
                    return true;
                }
            }
            return false;
        }
    }
}