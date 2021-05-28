//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Globalization;

namespace Nezaboodka.Text
{
    public sealed class Slice
    {
        // Properties

        public readonly string Source;
        public readonly int Position;
        public readonly int Length;

        public int End { get { return Position + Length - 1; } }

        public bool IsUndefined { get { return Source == null; } }

        public char this[int index]
        {
            get { if (index < Length) return Source[Position + index]; else throw new IndexOutOfRangeException(); }
        }

        // Procedures

        public Slice Clone()
        {
            return new Slice(this);
        }

        public override string ToString()
        {
            if (Source != null)
                if (Position == 0 && Length == Source.Length)
                    return Source;
                else
                    return Source.Substring(Position, Length);
            else
                return null;
        }

        public int CompareTo(Object value)
        {
            if (value != null)
            {
                if (value is Slice)
                    return Slice.InternalCompare(this, (Slice)value, CultureInfo.CurrentCulture,
                        CompareOptions.None);
                else if (value is String)
                    return Slice.InternalCompare(this, ((String)value).Slice(), CultureInfo.CurrentCulture,
                        CompareOptions.None);
                else
                    throw new ArgumentException("value must be of Slice or String type");
            }
            else
                return 1;
        }

        public int CompareTo(Slice sliceB)
        {
            return Slice.InternalCompare(this, sliceB, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public bool Contains(Slice value)
        {
            return (IndexOf(value, StringComparison.Ordinal) >= 0);
        }

        public bool StartsWith(Slice value)
        {
            return StartsWith(value, false, null);
        }

        public bool StartsWith(Slice value, StringComparison comparisonType)
        {
            if ((Object)value != null)
            {
                if ((Object)this != (Object)value)
                {
                    if (value.Length != 0)
                    {
                        if (value.Length <= Length)
                        {
                            switch (comparisonType)
                            {
                                case StringComparison.CurrentCulture:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.None) == 0;
                                case StringComparison.CurrentCultureIgnoreCase:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0;
                                case StringComparison.InvariantCulture:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.None) == 0;
                                case StringComparison.InvariantCultureIgnoreCase:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0;
                                case StringComparison.Ordinal:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.Ordinal) == 0;
                                case StringComparison.OrdinalIgnoreCase:
                                    return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                        CultureInfo.CurrentCulture, CompareOptions.OrdinalIgnoreCase) == 0;
                                default:
                                    throw new ArgumentOutOfRangeException("comparisonType");
                            }
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                throw new ArgumentNullException("value");
        }

        public bool StartsWith(Slice value, bool ignoreCase, CultureInfo culture)
        {
            if ((Object)value != null)
            {
                if ((Object)this != (Object)value)
                {
                    if (value.Length != 0)
                    {
                        if (value.Length <= Length)
                        {
                            CultureInfo referenceCulture = (culture == null) ? CultureInfo.CurrentCulture : culture;
                            CompareOptions compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
                            return Slice.InternalCompare(this, 0, value.Length, value, 0, value.Length,
                                referenceCulture, compareOptions) == 0;
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                throw new ArgumentNullException("value");
        }

        public bool StartsWith(char value)
        {
            if (Length > 0 && this[0] == value)
                return true;
            return false;
        }

        public bool EndsWith(Slice value)
        {
            return EndsWith(value, false, null);
        }

        public bool EndsWith(Slice value, StringComparison comparisonType)
        {
            if ((Object)value != null)
            {
                if ((Object)this != (Object)value)
                {
                    if (value.Length != 0)
                    {
                        if (value.Length <= Length)
                        {
                            switch (comparisonType)
                            {
                                case StringComparison.CurrentCulture:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.None) == 0;
                                case StringComparison.CurrentCultureIgnoreCase:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0;
                                case StringComparison.InvariantCulture:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.None) == 0;
                                case StringComparison.InvariantCultureIgnoreCase:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase) == 0;
                                case StringComparison.Ordinal:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.Ordinal) == 0;
                                case StringComparison.OrdinalIgnoreCase:
                                    return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                        value.Length, CultureInfo.CurrentCulture, CompareOptions.OrdinalIgnoreCase) == 0;
                                default:
                                    throw new ArgumentOutOfRangeException("comparisonType");
                            }
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                throw new ArgumentNullException("value");
        }

        public bool EndsWith(Slice value, bool ignoreCase, CultureInfo culture)
        {
            if ((Object)value != null)
            {
                if ((Object)this != (Object)value)
                {
                    if (value.Length != 0)
                    {
                        if (value.Length <= Length)
                        {
                            CultureInfo referenceCulture = (culture == null) ? CultureInfo.CurrentCulture : culture;
                            CompareOptions compareOptions = ignoreCase ? CompareOptions.IgnoreCase : CompareOptions.None;
                            return Slice.InternalCompare(this, Length - value.Length, value.Length, value, 0,
                                value.Length, referenceCulture, compareOptions) == 0;
                        }
                        else
                            return false;
                    }
                    else
                        return true;
                }
                else
                    return true;
            }
            else
                throw new ArgumentNullException("value");
        }

        public bool EndsWith(char value)
        {
            if (Length > 0 && this[Length - 1] == value)
                return true;
            return false;
        }

        public int IndexOf(char value)
        {
            return IndexOf(value, 0, Length);
        }

        public int IndexOf(char value, int startIndex)
        {
            return IndexOf(value, startIndex, Length - startIndex);
        }

        public int IndexOf(char value, int startIndex, int count)
        {
            if (startIndex >= 0)
            {
                if (count > Length - startIndex)
                    count = Length - startIndex;
                int result = Source.IndexOf(value, Position + startIndex, count);
                if (result >= 0)
                    result = result - Position;
                return result;
            }
            else
                throw new ArgumentOutOfRangeException("startIndex");
        }

        public int IndexOfAny(char[] anyOf)
        {
            return IndexOfAny(anyOf, 0, Length);
        }

        public int IndexOfAny(char[] anyOf, int startIndex)
        {
            return IndexOfAny(anyOf, startIndex, Length - startIndex);
        }

        public int IndexOfAny(char[] anyOf, int startIndex, int count)
        {
            if (startIndex >= 0)
            {
                if (count > Length - startIndex)
                    count = Length - startIndex;
                int result = Source.IndexOfAny(anyOf, Position + startIndex, count);
                if (result >= 0)
                    result = result - Position;
                return result;
            }
            else
                throw new ArgumentOutOfRangeException("startIndex");
        }

        public int IndexOf(Slice value)
        {
            return InternalIndexOf(value, 0, Length, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public int IndexOf(Slice value, int startIndex)
        {
            return InternalIndexOf(value, startIndex, Length - startIndex, CultureInfo.CurrentCulture,
                CompareOptions.None);
        }

        public int IndexOf(Slice value, int startIndex, int count)
        {
            return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public int IndexOf(Slice value, StringComparison comparisonType)
        {
            return IndexOf(value, 0, Length, comparisonType);
        }

        public int IndexOf(Slice value, int startIndex, StringComparison comparisonType)
        {
            return IndexOf(value, startIndex, Length - startIndex, comparisonType);
        }

        public int IndexOf(Slice value, int startIndex, int count, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.None);
                case StringComparison.CurrentCultureIgnoreCase:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.IgnoreCase);
                case StringComparison.InvariantCulture:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.None);
                case StringComparison.InvariantCultureIgnoreCase:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.IgnoreCase);
                case StringComparison.Ordinal:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.Ordinal);
                case StringComparison.OrdinalIgnoreCase:
                    return InternalIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }
        }

        public int LastIndexOf(char value)
        {
            return LastIndexOf(value, Length - 1, Length);
        }

        public int LastIndexOf(char value, int startIndex)
        {
            return LastIndexOf(value, startIndex, startIndex + 1);
        }

        public int LastIndexOf(char value, int startIndex, int count)
        {
            if (Length > 0)
            {
                if (startIndex >= 0 && startIndex <= Length)
                {
                    if (startIndex == Length)
                    {
                        startIndex--;
                        if (count > 0)
                            count--;
                    }
                    if (count > Length)
                        count = Length;
                    int result = Source.LastIndexOf(value, Position + startIndex, count);
                    if (result >= 0)
                        result = result - Position;
                    return result;
                }
                else
                    throw new ArgumentOutOfRangeException("startIndex");
            }
            else if (startIndex == 0 || startIndex == -1)
                return -1;
            else
                throw new ArgumentOutOfRangeException("startIndex");
        }

        public int LastIndexOfAny(char[] anyOf)
        {
            return LastIndexOfAny(anyOf, Length - 1, Length);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex)
        {
            return LastIndexOfAny(anyOf, startIndex, startIndex + 1);
        }

        public int LastIndexOfAny(char[] anyOf, int startIndex, int count)
        {
            if (Length > 0)
            {
                if (startIndex >= 0 && startIndex <= Length)
                {
                    if (startIndex == Length)
                    {
                        startIndex--;
                        if (count > 0)
                            count--;
                    }
                    if (count > Length)
                        count = Length;
                    int result = Source.LastIndexOfAny(anyOf, Position + startIndex, count);
                    if (result >= 0)
                        result = result - Position;
                    return result;
                }
                else
                    throw new ArgumentOutOfRangeException("startIndex");
            }
            else if (startIndex == 0 || startIndex == -1)
                return -1;
            else
                throw new ArgumentOutOfRangeException("startIndex");
        }

        public int LastIndexOf(Slice value)
        {
            return InternalLastIndexOf(value, Length - 1, Length, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public int LastIndexOf(Slice value, int startIndex)
        {
            return InternalLastIndexOf(value, startIndex, startIndex + 1, CultureInfo.CurrentCulture,
                CompareOptions.None);
        }

        public int LastIndexOf(Slice value, int startIndex, int count)
        {
            return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public int LastIndexOf(Slice value, StringComparison comparisonType)
        {
            return LastIndexOf(value, Length - 1, Length, comparisonType);
        }

        public int LastIndexOf(Slice value, int startIndex, StringComparison comparisonType)
        {
            return LastIndexOf(value, startIndex, startIndex + 1, comparisonType);
        }

        public int LastIndexOf(Slice value, int startIndex, int count, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.None);
                case StringComparison.CurrentCultureIgnoreCase:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.IgnoreCase);
                case StringComparison.InvariantCulture:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.None);
                case StringComparison.InvariantCultureIgnoreCase:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.IgnoreCase);
                case StringComparison.Ordinal:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.Ordinal);
                case StringComparison.OrdinalIgnoreCase:
                    return InternalLastIndexOf(value, startIndex, count, CultureInfo.CurrentCulture,
                        CompareOptions.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }
        }

        public Slice Trim(params char[] trimChars)
        {
            if (null == trimChars || trimChars.Length == 0)
                trimChars = Whitespaces;
            return InternalTrim(trimChars, TrimKind.Both);
        }

        public Slice TrimStart(params char[] trimChars)
        {
            if (null == trimChars || trimChars.Length == 0)
                trimChars = Whitespaces;
            return InternalTrim(trimChars, TrimKind.Head);
        }

        public Slice TrimEnd(params char[] trimChars)
        {
            if (null == trimChars || trimChars.Length == 0)
                trimChars = Whitespaces;
            return InternalTrim(trimChars, TrimKind.Tail);
        }

        public Slice[] Split(params char[] separator)
        {
            return Split(separator, Int32.MaxValue, StringSplitOptions.None);
        }

        public Slice[] Split(char[] separator, int count)
        {
            return Split(separator, count, StringSplitOptions.None);
        }

        public Slice[] Split(char[] separator, StringSplitOptions options)
        {
            return Split(separator, Int32.MaxValue, options);
        }

        public Slice[] Split(char[] separator, int count, StringSplitOptions options)
        {
            if (count >= 0)
            {
                if (options == StringSplitOptions.None || options == StringSplitOptions.RemoveEmptyEntries)
                {
                    if (count > 0 && (options == StringSplitOptions.None || Length > 0))
                    {
                        int[] sepList = new int[Length];
                        int numReplaces = MakeSeparatorList(separator, ref sepList);
                        if (numReplaces > 0 && count > 1)
                        {
                            if (options == StringSplitOptions.None)
                                return InternalSplitKeepEmptyEntries(sepList, null, numReplaces, count);
                            else
                                return InternalSplitOmitEmptyEntries(sepList, null, numReplaces, count);
                        }
                        else
                        {
                            Slice[] sliceArray = new Slice[1];
                            sliceArray[0] = this;
                            return sliceArray;
                        }
                    }
                    else
                        return new Slice[0];
                }
                else
                    throw new ArgumentOutOfRangeException("options");
            }
            else
                throw new ArgumentOutOfRangeException("count");
        }

        public Slice[] Split(String[] separator, StringSplitOptions options)
        {
            return Split(separator, Int32.MaxValue, options);
        }

        public Slice[] Split(String[] separator, int count, StringSplitOptions options)
        {
            if (count >= 0)
            {
                if (options == StringSplitOptions.None || options == StringSplitOptions.RemoveEmptyEntries)
                {
                    if (separator != null && separator.Length > 0)
                    {
                        if (count > 0 && (options == StringSplitOptions.None || Length > 0))
                        {
                            int[] sepList = new int[Length];
                            int[] lengthList = new int[Length];
                            int numReplaces = MakeSeparatorList(separator, ref sepList, ref lengthList);
                            if (numReplaces > 0 && count > 1)
                            {
                                if (options == StringSplitOptions.None)
                                    return InternalSplitKeepEmptyEntries(sepList, lengthList, numReplaces, count);
                                else
                                    return InternalSplitOmitEmptyEntries(sepList, lengthList, numReplaces, count);
                            }
                            else
                            {
                                Slice[] sliceArray = new Slice[1];
                                sliceArray[0] = this;
                                return sliceArray;
                            }
                        }
                        else
                            return new Slice[0];
                    }
                    else
                        return Split((char[])null, count, options);
                }
                else
                    throw new ArgumentOutOfRangeException("options");
            }
            else
                throw new ArgumentOutOfRangeException("count");
        }

        public Slice HeadSibling()
        {
            return Source.Slice(0, Position);
        }

        public Slice TailSibling()
        {
            return Source.Slice(Position + Length);
        }

        public static readonly char[] Whitespaces =
        {
            (char)0x9, (char)0xA, (char)0xB, (char)0xC, (char)0xD, (char)0x20, (char)0x85,
            (char)0xA0, (char)0x1680, (char)0x2000, (char)0x2001, (char)0x2002, (char)0x2003,
            (char)0x2004, (char)0x2005, (char)0x2006, (char)0x2007, (char)0x2008, (char)0x2009,
            (char)0x200A, (char)0x200B, (char)0x2028, (char)0x2029, (char)0x3000, (char)0xFEFF
        };

        public static bool IsNull(Slice slice)
        {
            return slice == null; // slice.Source == null;
        }

        public static bool IsNullOrEmpty(Slice slice)
        {
            return slice == null || slice.Length == 0 || slice.Source == null;
        }

        public static int Compare(Slice sliceA, Slice sliceB)
        {
            return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, Slice sliceB, bool ignoreCase)
        {
            if (ignoreCase)
                return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
            else
                return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, Slice sliceB, StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.None);
                case StringComparison.CurrentCultureIgnoreCase:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
                case StringComparison.InvariantCulture:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.None);
                case StringComparison.InvariantCultureIgnoreCase:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
                case StringComparison.Ordinal:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.Ordinal);
                case StringComparison.OrdinalIgnoreCase:
                    return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }
        }

        public static int Compare(Slice sliceA, Slice sliceB, bool ignoreCase, CultureInfo culture)
        {
            if (ignoreCase)
                return InternalCompare(sliceA, sliceB, culture, CompareOptions.IgnoreCase);
            else
                return InternalCompare(sliceA, sliceB, culture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, int indexA, Slice sliceB, int indexB, int length)
        {
            return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, int indexA, Slice sliceB, int indexB, int length, bool ignoreCase)
        {
            if (ignoreCase)
                return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
            else
                return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                    CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, int indexA, Slice sliceB, int indexB, int length, bool ignoreCase,
            CultureInfo culture)
        {
            if (ignoreCase)
                return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                    CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
            else
                return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                    CultureInfo.CurrentCulture, CompareOptions.None);
        }

        public static int Compare(Slice sliceA, int indexA, Slice sliceB, int indexB, int length,
            StringComparison comparisonType)
        {
            switch (comparisonType)
            {
                case StringComparison.CurrentCulture:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.None);
                case StringComparison.CurrentCultureIgnoreCase:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
                case StringComparison.InvariantCulture:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.None);
                case StringComparison.InvariantCultureIgnoreCase:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.IgnoreCase);
                case StringComparison.Ordinal:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.Ordinal);
                case StringComparison.OrdinalIgnoreCase:
                    return InternalCompare(sliceA, indexA, length, sliceB, indexB, length,
                        CultureInfo.CurrentCulture, CompareOptions.OrdinalIgnoreCase);
                default:
                    throw new ArgumentOutOfRangeException("comparisonType");
            }
        }

        public static int CompareOrdinal(Slice sliceA, Slice sliceB)
        {
            return InternalCompare(sliceA, sliceB, CultureInfo.CurrentCulture, CompareOptions.Ordinal);
        }

        public static int CompareOrdinal(Slice sliceA, int indexA, Slice sliceB, int indexB, int length)
        {
            return InternalCompare(sliceA, indexA, length, sliceB, indexB, length, CultureInfo.CurrentCulture,
                CompareOptions.Ordinal);
        }

        // Internals

        public Slice()
        {
        }

        public Slice(string source)
            : this(source, 0, source.Length)
        {
        }

        public Slice(string source, int position)
            : this(source, position, source.Length - position)
        {
        }

        public Slice(string source, int position, int length)
        {
            if (position >= 0 && length >= 0 && position + length <= source.Length)
            {
                Source = source;
                Position = position;
                Length = length;
            }
            else
                throw new ArgumentException("wrong slice");
        }

        public Slice(Slice slice)
        {
            Source = slice.Source;
            Position = slice.Position;
            Length = slice.Length;
        }

        public Slice(Slice slice, int position)
            : this(slice, position, slice.Length - position)
        {
        }

        public Slice(Slice slice, int position, int length)
        {
            if (position >= 0 && length >= 0 && position + length <= slice.Length)
            {
                Source = slice.Source;
                Position = slice.Position + position;
                Length = length;
            }
            else
                throw new ArgumentException("wrong slice");
        }

        private int InternalIndexOf(Slice value, int startIndex, int count, CultureInfo culture, CompareOptions options)
        {
            if (culture != null)
            {
                if (!IsNull(value))
                {
                    if (startIndex >= 0 && startIndex <= Length)
                    {
                        if (count >= 0)
                        {
                            if (count > Length - startIndex)
                                count = Length - startIndex;
                            // Workaround: when searching for slice, we convert it to string,
                            // as there's no built-in procedure, which can search for string part.
                            int result = culture.CompareInfo.IndexOf(Source, value.ToString(),
                                Position + startIndex, count, CompareOptions.None);
                            if (result >= 0)
                                result = result - Position;
                            return result;
                        }
                        else
                            throw new ArgumentOutOfRangeException("count");
                    }
                    else
                        throw new ArgumentOutOfRangeException("startIndex");
                }
                else
                    throw new ArgumentNullException("value or value.Source");
            }
            else
                throw new ArgumentNullException("culture");
        }

        private int InternalLastIndexOf(Slice value, int startIndex, int count, CultureInfo culture,
            CompareOptions options)
        {
            if (culture != null)
            {
                if (!IsNull(value))
                {
                    if (Length > 0)
                    {
                        if (startIndex >= 0 && startIndex <= Length)
                        {
                            if (startIndex == Length)
                            {
                                startIndex--;
                                if (count > 0)
                                    count--;
                            }
                            if (count >= 0)
                            {
                                if (count > Length)
                                    count = Length;
                                // Workaround: when searching for slice, we convert it to string,
                                // as there's no built-in procedure, which can search for string part.
                                int result = culture.CompareInfo.LastIndexOf(Source, value.ToString(),
                                    Position + startIndex, count, CompareOptions.None);
                                if (result >= 0)
                                    result = result - Position;
                                return result;
                            }
                            else
                                throw new ArgumentOutOfRangeException("count");
                        }
                        else
                            throw new ArgumentOutOfRangeException("startIndex");
                    }
                    else if (startIndex == 0 || startIndex == -1)
                        return (value.Length == 0) ? 0 : -1;
                    else
                        throw new ArgumentOutOfRangeException("startIndex");
                }
                else
                    throw new ArgumentNullException("value or value.Source");
            }
            else
                throw new ArgumentNullException("culture");
        }

        private enum TrimKind { Head, Tail, Both }

        private Slice InternalTrim(char[] trimChars, TrimKind trimKind)
        {
            int start = Position;
            int end = End;
            if (trimKind != TrimKind.Tail)
            {
                for (start = Position; start <= end; start++)
                {
                    int i = 0;
                    char ch = Source[start];
                    for (i = 0; i < trimChars.Length; i++)
                        if (trimChars[i] == ch)
                            break;
                    if (i == trimChars.Length)
                        break;
                }
            }
            if (trimKind != TrimKind.Head)
            {
                for (end = End; end >= start; end--)
                {
                    int i = 0;
                    char ch = Source[end];
                    for (i = 0; i < trimChars.Length; i++)
                        if (trimChars[i] == ch)
                            break;
                    if (i == trimChars.Length)
                        break;
                }
            }
            return new Slice(Source, start, end - start + 1);
        }

        private Slice[] InternalSplitKeepEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
        {
            int currIndex = 0;
            int arrIndex = 0;
            count--;
            int numActualReplaces = (numReplaces < count) ? numReplaces : count;
            Slice[] splitSlices = new Slice[numActualReplaces + 1];
            for (int i = 0; i < numActualReplaces && currIndex < Length; i++)
            {
                splitSlices[arrIndex++] = this.SubSlice(currIndex, sepList[i] - currIndex);
                currIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
            }
            if (currIndex < Length && numActualReplaces >= 0)
            {
                splitSlices[arrIndex] = this.SubSlice(currIndex);
            }
            else if (arrIndex == numActualReplaces)
            {
                splitSlices[arrIndex] = this.SubSlice(0, 0);
            }
            return splitSlices;
        }

        private Slice[] InternalSplitOmitEmptyEntries(int[] sepList, int[] lengthList, int numReplaces, int count)
        {
            int maxItems = (numReplaces < count) ? (numReplaces + 1) : count;
            Slice[] splitSlices = new Slice[maxItems];
            int currIndex = 0;
            int arrIndex = 0;
            for (int i = 0; i < numReplaces && currIndex < Length; i++)
            {
                if (sepList[i] - currIndex > 0)
                    splitSlices[arrIndex++] = this.SubSlice(currIndex, sepList[i] - currIndex);
                currIndex = sepList[i] + ((lengthList == null) ? 1 : lengthList[i]);
                if (arrIndex == count - 1)
                {
                    while (i < numReplaces - 1 && currIndex == sepList[++i])
                        currIndex += ((lengthList == null) ? 1 : lengthList[i]);
                    break;
                }
            }
            if (currIndex < Length)
                splitSlices[arrIndex++] = this.SubSlice(currIndex);
            Slice[] sliceArray = splitSlices;
            if (arrIndex != maxItems)
            {
                sliceArray = new Slice[arrIndex];
                for (int j = 0; j < arrIndex; j++)
                    sliceArray[j] = splitSlices[j];
            }
            return sliceArray;
        } 

        private int MakeSeparatorList(char[] separator, ref int[] sepList)
        {
            int foundCount = 0;
            if (separator != null && separator.Length > 0)
            {
                int sepListCount = sepList.Length;
                int sepCount = separator.Length;
                for (int i = 0; i < Length && foundCount < sepListCount; i++)
                {
                    for (int j = 0; j < sepCount; j++)
                    {
                        if (this[i] == separator[j])
                        {
                            sepList[foundCount++] = i;
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < Length && foundCount < sepList.Length; i++)
                    if (Char.IsWhiteSpace(this[i]))
                        sepList[foundCount++] = i;
            }
            return foundCount;
        }

        private int MakeSeparatorList(String[] separators, ref int[] sepList, ref int[] lengthList)
        {
            int foundCount = 0;
            int sepListCount = sepList.Length;
            for (int i = 0; i < Length && foundCount < sepListCount; i++)
            {
                for (int j = 0; j < separators.Length; j++)
                {
                    String separator = separators[j];
                    if (!string.IsNullOrEmpty(separator))
                    {
                        int currentSepLength = separator.Length;
                        if (this[i] == separator[0] && currentSepLength <= Length - i)
                        {
                            if (currentSepLength == 1 ||
                                string.CompareOrdinal(Source, Position + i, separator, 0, currentSepLength) == 0)
                            {
                                sepList[foundCount] = i;
                                lengthList[foundCount] = currentSepLength;
                                foundCount++;
                                i += currentSepLength - 1;
                                break;
                            }
                        }
                    }
                    else
                        continue;
                }
            }
            return foundCount;
        } 

        // Static internals

        private static int InternalCompare(Slice sliceA, Slice sliceB, CultureInfo culture, CompareOptions options)
        {
            if (culture != null)
            {
                if ((Object)sliceA != (Object)sliceB)
                {
                    if (!IsNull(sliceA))
                    {
                        if (!IsNull(sliceB))
                        {
                            return culture.CompareInfo.Compare(
                                sliceA.Source, sliceA.Position, sliceA.Length,
                                sliceB.Source, sliceB.Position, sliceB.Length, options);
                        }
                        else if (sliceA.Source != null)
                            return 1;
                        else
                            return 0;
                    }
                    else if (sliceB.Source != null)
                        return -1;
                    else
                        return 0;
                }
                else
                    return 0;
            }
            else
                throw new ArgumentNullException("culture");
        }

        private static int InternalCompare(Slice sliceA, int indexA, int lengthA,
            Slice sliceB, int indexB, int lengthB, CultureInfo culture, CompareOptions options)
        {
            if (culture != null)
            {
                if (indexA >= 0 && lengthA >= 0 && indexB >= 0 && lengthB >= 0)
                {
                    if ((Object)sliceA != (Object)sliceB)
                    {
                        if (!IsNull(sliceA))
                        {
                            if (!IsNull(sliceB))
                            {
                                if (lengthA > sliceA.Length - indexA)
                                    lengthA = sliceA.Length - indexA;
                                if (lengthB > sliceB.Length - indexB)
                                    lengthB = sliceB.Length - indexB;
                                return culture.CompareInfo.Compare(
                                    sliceA.Source, sliceA.Position + indexA, lengthA,
                                    sliceB.Source, sliceB.Position + indexB, lengthB, options);
                            }
                            else if (sliceA.Source != null)
                                return 1;
                            else
                                return 0;
                        }
                        else if (sliceB.Source != null)
                            return -1;
                        else
                            return 0;
                    }
                    else
                        return 0;
                }
                else
                    throw new ArgumentOutOfRangeException("indexA or lengthA or indexB or lengthB");
            }
            else
                throw new ArgumentNullException("culture");
        }

    }

    public static class SliceUtilities
    {
        public static Slice Slice(this string str)
        {
            return new Slice(str);
        }

        public static Slice Slice(this string str, int position)
        {
            return new Slice(str, position);
        }

        public static Slice Slice(this string str, int position, int length)
        {
            return new Slice(str, position, length);
        }

        public static Slice SubSlice(this Slice slice, int position)
        {
            return new Slice(slice, position);
        }

        public static Slice SubSlice(this Slice slice, int position, int length)
        {
            return new Slice(slice, position, length);
        }

        public static Slice SliceUntil(this Slice slice, int position, bool required, bool searchFromRightToLeft,
            Slice until)
        {
            int i = slice.Length;
            if (!Text.Slice.IsNullOrEmpty(until) && until.Length > 0)
            {
                if (!searchFromRightToLeft)
                    i = slice.IndexOf(until, position);
                else
                    i = slice.LastIndexOf(until, slice.Length - 1, slice.Length - position);
            }
            if (i >= position)
                return slice.SubSlice(position, i - position);
            else
            {
                if (!required)
                    return new Slice();
                else
                    throw new ArgumentException(string.Format(
                        "'{0}' not found after startPosition {1}", until, position));
            }
        }

        //public static Slice SliceUntilAny(this string str, int startPosition, bool required, bool searchFromRightToLeft, params Slice[] until)
        //{
        //    var end = str.Length - 1;
        //    var xuntil = default(string[]);
        //    if (until != null && until.Length > 0)
        //    {
        //        xuntil = until.Select((Slice x) => x.ToString()).ToArray();
        //        throw new NotImplementedException();
        //        //if (!searchFromRightToLeft)
        //        //    end = str.IndexOfAny(xuntil, startPosition);
        //        //else
        //        //    end = str.LastIndexOfAny(xuntil, str.Length - 1);
        //    }
        //    if (end < startPosition && required)
        //        throw new ArgumentException(string.Format(
        //            "'{0}' not found after startPosition {1}", xuntil, startPosition));
        //    return str.Slice(startPosition, end - startPosition);
        //}
    }
}
