//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public static class SortedListExtension
    {
        public static void Search<TKey, TValue>(this SortedList<TKey, TValue> sortedList,
            TKey key, Func<TKey, TKey, int> keyToItemComparer, out int start, out int end)
        {
            int middle = sortedList.Keys.BinarySearch(key, keyToItemComparer);
            if (middle >= 0)
            {
                end = middle + 1;
                start = middle - 1;
            }
            else
            {
                end = ~middle;
                start = end - 1;
            }
            while (start >= 0 && keyToItemComparer(key, sortedList.Keys[start]) == 0)
                start--;
            start++;
            int count = sortedList.Count;
            while (end < count && keyToItemComparer(key, sortedList.Keys[end]) == 0)
                end++;
        }

        public static SortedList<TKey, List<TValue>> MergeFromNullable<TKey, TValue>(
            this SortedList<TKey, List<TValue>> destination, SortedList<TKey, List<TValue>> source,
            Func<SortedList<TKey, List<TValue>>> sortedListConstructor)
        {
            SortedList<TKey, List<TValue>> result;
            if (source != null)
            {
                if (destination == null)
                    result = sortedListConstructor();
                else
                    result = destination;
                foreach (KeyValuePair<TKey, List<TValue>> keyValuePair in source)
                {
                    if (result.TryGetValue(keyValuePair.Key, out List<TValue> expressionsList))
                        expressionsList.AddRange(keyValuePair.Value);
                    else
                        result[keyValuePair.Key] = new List<TValue>(keyValuePair.Value);
                }
            }
            else
                result = destination;
            return result;
        }

        public static SortedList<TKey, ImmutableArray<TValue>> MergeFromNullable<TKey, TValue>(
            this SortedList<TKey, ImmutableArray<TValue>> destination, SortedList<TKey, ImmutableArray<TValue>> source,
            Func<SortedList<TKey, ImmutableArray<TValue>>> sortedListConstructor)
        {
            SortedList<TKey, ImmutableArray<TValue>> result;
            if (source != null)
            {
                if (destination == null)
                    result = sortedListConstructor();
                else
                    result = destination;
                foreach (KeyValuePair<TKey, ImmutableArray<TValue>> keyValuePair in source)
                {
                    if (result.TryGetValue(keyValuePair.Key, out ImmutableArray<TValue> expressionsList))
                        result[keyValuePair.Key] = new ImmutableArray<TValue>(expressionsList, keyValuePair.Value);
                    else
                        result[keyValuePair.Key] = new ImmutableArray<TValue>(keyValuePair.Value);
                }
            }
            else
                result = destination;
            return result;
        }
    }

    public static class IListExtension
    {
        public static int BinarySearch<TItem, TSearchValue>(this IList<TItem> list, TSearchValue value,
            Func<TSearchValue, TItem, int> comparer = null)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));
            int lower = 0;
            int upper = list.Count - 1;
            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                int comparisonResult = comparer(value, list[middle]);
                if (comparisonResult < 0)
                    upper = middle - 1;
                else if (comparisonResult > 0)
                    lower = middle + 1;
                else // if (comparisonResult == 0)
                    return middle;
            }
            return ~lower;
        }

        public static int BinarySearch<TItem>(this IList<TItem> list, TItem value)
        {
            return BinarySearch(list, value, Comparer<TItem>.Default);
        }

        public static int BinarySearch<TItem>(this IList<TItem> list, TItem value, IComparer<TItem> comparer)
        {
            return BinarySearch(list, value, comparer.Compare);
        }
    }
}
