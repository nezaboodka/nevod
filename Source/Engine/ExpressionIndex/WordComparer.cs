//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;

namespace Nezaboodka.Nevod
{
    internal static class WordComparer
    {
        public static int WordToPrefixComparer(string word, string prefix)
        {
            int result;
            if (word.Length >= prefix.Length)
                result = string.Compare(word, 0, prefix, 0, prefix.Length, StringComparison.OrdinalIgnoreCase);
            else
                result = string.Compare(word, prefix, StringComparison.OrdinalIgnoreCase);
            return result;
        }

        public static bool WordPrefixAttributesEqualityComparer(Token token, TokenExpression sample)
        {
            if (sample.IsCaseSensitive
                && string.CompareOrdinal(token.Text, 0, sample.Text, 0, sample.Text.Length) != 0)
                return false;
            return sample.TokenAttributes == null || sample.TokenAttributes.CompareTo(token, sample.Text);
        }

        private static bool IsHexCharacter(char c)
        {
            switch (c)
            {
                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                    return true;
                default:
                    return false;
            }
        }
    }
}
