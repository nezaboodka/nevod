//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Text
{
    public static class CollectionExtension
    {
        public static void ClearAndAdd<T>(this ICollection<T> collection, params T[] items)
        {
            collection.Clear();
            collection.AddRange(items);
        }

        public static void Add<T>(this ICollection<T> collection, params T[] items)
        {
            collection.AddRange(items);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> from)
        {
            if (from != null)
                foreach (T x in from)
                    collection.Add(x);
        }

        public static void AddRange(this IList collection, IEnumerable from)
        {
            if (from != null)
                foreach (object x in from)
                    collection.Add(x);
        }
    }
}
