//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Nezaboodka.Nevod.Negrep
{
    public static class StringExtensions
    {
        public static string TrimEachLine(this string str)
        {
            return $"{Regex.Replace(str, @"\n\s+", "\n").Trim()}";
        }

        public static string AddLineBreak(this string str)
        {
            return $"{str}{Environment.NewLine}";
        }

        public static string ReplaceLineBreakWithNull(this string str)
        {
            return Regex.Replace(str, @"\r\n|\r|\n", "\0");
        }

        public static IEnumerable<string> SortAllLinesByHashCode(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            return str.Split(new string[] { Environment.NewLine }, StringSplitOptions.None)
                .OrderBy(line => line.GetHashCode());
        }

        public static bool BeginsWithDashOrDoubleDash(this string str)
        {
            if (str == null)
                throw new ArgumentNullException(nameof(str));
            var dashes = str.TakeWhile(ch => ch == '-');
            return dashes.Count() > 0 && dashes.Count() < 3;
        }

        public static bool IsGlob(this string str)
        {
            return str.Contains('*');
        }
    }
}
