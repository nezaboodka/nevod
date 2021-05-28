//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Text
{
    public static class EnumerableExtension
    {
        public static string ToString<T>(this IEnumerable<T> items, string delimiter)
        {
            return items.ToString("{0}", delimiter);
        }

        public static string ToString<T>(this IEnumerable<T> items, string itemFormat, string delimiter)
        {
            var sb = new StringBuilder();
            foreach (var x in items)
            {
                if (sb.Length > 0)
                    sb.Append(delimiter);
                sb.AppendFormat(itemFormat, x.ToString());
            }
            return sb.ToString();
        }
    }
}
