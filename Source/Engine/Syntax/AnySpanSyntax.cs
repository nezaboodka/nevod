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
    public class AnySpanSyntax : Syntax
    {
        public Syntax Left { get; }
        public Syntax Right { get; }
        public Syntax ExtractionOfSpan { get; }

        internal AnySpanSyntax(Syntax left, Syntax right, Syntax extractionOfSpan)
        {
            Left = left;
            Right = right;
            ExtractionOfSpan = extractionOfSpan;
        }

        internal AnySpanSyntax Update(Syntax left, Syntax right, Syntax extractionOfSpan)
        {
            AnySpanSyntax result = this;
            if (left != Left || right != Right || extractionOfSpan != ExtractionOfSpan)
                result = AnySpan(left, right, extractionOfSpan);
            return result;
        }

        protected internal override Syntax Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitAnySpan(this);
        }
    }

    public partial class Syntax
    {
        public static AnySpanSyntax AnySpan(Syntax left, Syntax right)
        {
            var result = new AnySpanSyntax(left, right, null);
            return result;
        }

        public static AnySpanSyntax AnySpan(Syntax left, Syntax right, Syntax extractionOfSpan)
        {
            var result = new AnySpanSyntax(left, right, extractionOfSpan);
            return result;
        }
    }
}
