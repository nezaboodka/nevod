//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    public static class ArrayDictionaryExtension
    {
        public static void AddValueListItem<TValue>(this ImmutableArray<TValue>[] dictionary,
            int key, TValue value)
        {
            ImmutableArray<TValue> valueList = dictionary[key];
            dictionary[key] = new ImmutableArray<TValue>(valueList, value);
        }

        public static void AddValueListItem<TValue>(this ImmutableArray<TValue>[][] dictionary,
            char key, TValue value)
        {
            int i = key >> 8;
            ImmutableArray<TValue>[] list = dictionary[i];
            if (list == null)
            {
                list = new ImmutableArray<TValue>[256];
                dictionary[i] = list;
            }
            int j = key & 0x00FF;
            list[j] = new ImmutableArray<TValue>(list[j], value);
        }

        public static ImmutableArray<TValue>[] MergeFromNullable<TValue>(this ImmutableArray<TValue>[] destination,
            ImmutableArray<TValue>[] source)
        {
            ImmutableArray<TValue>[] result;
            if (source != null)
            {
                if (destination == null)
                    result = new ImmutableArray<TValue>[source.Length];
                else
                    result = destination;
                for (int i = 0, n = source.Length; i < n; i++)
                {
                    ImmutableArray<TValue> sourceValues = source[i];
                    result[i] = new ImmutableArray<TValue>(result[i], source[i]);
                }
            }
            else
                result = destination;
            return result;
        }

        public static ImmutableArray<TValue>[][] MergeFromNullable<TValue>(this ImmutableArray<TValue>[][] destination,
            ImmutableArray<TValue>[][] source)
        {
            ImmutableArray<TValue>[][] result;
            if (source != null)
            {
                if (destination == null)
                    result = new ImmutableArray<TValue>[source.Length][];
                else
                    result = destination;
                for (int i = 0, n = source.Length; i < n; i++)
                    result[i] = result[i].MergeFromNullable(source[i]);
            }
            else
                result = destination;
            return result;
        }
    }
}
