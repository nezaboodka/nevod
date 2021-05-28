//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Nezaboodka.Nevod
{
    public class WordSpanSyntax : Syntax
    {
        public Syntax Left { get; }
        public Range SpanRange { get; }
        public Syntax Right { get; }
        public Syntax Exclusion { get; }
        public Syntax ExtractionOfSpan { get; }

        internal WordSpanSyntax(Syntax left, Range spanRange, Syntax right, Syntax exclusion, Syntax extractionOfSpan)
        {
            Left = left;
            SpanRange = spanRange;
            Right = right;
            Exclusion = exclusion;
            ExtractionOfSpan = extractionOfSpan;
        }

        internal WordSpanSyntax(Syntax left, int spanRangeLowBound, int spanRangeHighBound, Syntax right,
            Syntax exclusion, Syntax extractionOfSpan)
            : this(left, new Range(spanRangeLowBound, spanRangeHighBound), right, exclusion, extractionOfSpan)
        {
        }

        internal WordSpanSyntax Update(Syntax left, Syntax right, Syntax exclusion, Syntax extractionOfSpan)
        {
            WordSpanSyntax result = this;
            if (left != Left || right != Right || exclusion != Exclusion || extractionOfSpan != ExtractionOfSpan)
                result = WordSpan(left, SpanRange, right, exclusion, extractionOfSpan);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitWordSpan(this);
        }
    }

    public partial class Syntax
    {
        public static WordSpanSyntax WordSpan(Syntax left, Syntax right)
        {
            var result = new WordSpanSyntax(left, Range.ZeroPlus(), right, null, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, Range spanRange, Syntax right)
        {
            var result = new WordSpanSyntax(left, spanRange, right, null, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, int spanRangeHighBound, Syntax right)
        {
            var result = new WordSpanSyntax(left, 0, spanRangeHighBound, right, null, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, int spanRangeLowBound, int spanRangeHighBound, Syntax right)
        {
            var result = new WordSpanSyntax(left, spanRangeLowBound, spanRangeHighBound, right, null, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, int spanRangeLowBound, int spanRangeHighBound, Syntax right,
            Syntax exclusion)
        {
            var result = new WordSpanSyntax(left, spanRangeLowBound, spanRangeHighBound, right, exclusion, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, Range spanRange, Syntax right,
            Syntax exclusion)
        {
            var result = new WordSpanSyntax(left, spanRange, right, exclusion, null);
            return result;
        }

        public static WordSpanSyntax WordSpan(Syntax left, Range spanRange, Syntax right,
            Syntax exclusion, Syntax extractionOfSpan)
        {
            var result = new WordSpanSyntax(left, spanRange, right, exclusion, extractionOfSpan);
            return result;
        }
    }
}
