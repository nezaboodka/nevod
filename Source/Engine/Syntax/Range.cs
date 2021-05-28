//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    public struct Range
    {
        public const int Max = int.MaxValue;

        public int LowBound;
        public int HighBound;

        public Range(int lowBound, int highBound)
        {
            LowBound = lowBound;
            HighBound = highBound;
        }

        public bool IsZero()
        {
            return LowBound == 0 && HighBound == 0;
        }

        public bool IsZeroToOne()
        {
            return LowBound == 0 && HighBound == 1;
        }

        public bool IsZeroPlus()
        {
            return LowBound == 0 && HighBound == Max;
        }

        public bool IsOnePlus()
        {
            return LowBound == 1 && HighBound == Max;
        }

        public bool IsOne()
        {
            return LowBound == 1 && HighBound == 1;
        }

        public bool IsSingleValue()
        {
            return LowBound == HighBound && HighBound != Max;
        }

        public bool IsZeroPlusOrOnePlus()
        {
            return (LowBound == 0 || LowBound == 1) && HighBound == Max;
        }

        public static Range ZeroToOne()
        {
            return new Range(0, 1);
        }

        public static Range ZeroPlus()
        {
            return new Range(0, Max);
        }

        public static Range OnePlus()
        {
            return new Range(1, Max);
        }

        public static Range One()
        {
            return new Range(1, 1);
        }

        public static Range SingleValue(int value)
        {
            return new Range(value, value);
        }

        public override bool Equals(object obj)
        {
            bool result = false;
            if (obj != null && obj is Range other)
                result = LowBound == other.LowBound && HighBound == other.HighBound;
            return result;
        }

        public override int GetHashCode()
        {
            return LowBound + HighBound;
        }

        public bool Equals(Range obj)
        {
            return this == obj;
        }

        public static bool operator ==(Range x, Range y)
        {
            return x.LowBound == y.LowBound && x.HighBound == y.HighBound;
        }

        public static bool operator !=(Range x, Range y)
        {
            return !(x == y);
        }
    }
}
