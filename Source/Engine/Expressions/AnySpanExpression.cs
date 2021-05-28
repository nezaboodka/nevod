//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal class AnySpanExpression : CompoundExpression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        public Range SpanRangeInWords { get; }
        public Expression ExtractionOfSpan { get; }

        public AnySpanExpression(Syntax syntax, Expression left, Expression right, Range spanRangeInWords,
            Expression extractionOfSpan)
            : base(syntax)
        {
            Left = left;
            Left.SetParentExpression(this, LeftPosition);
            Right = right;
            Right.SetParentExpression(this, RightPosition);
            SpanRangeInWords = spanRangeInWords;
            ExtractionOfSpan = extractionOfSpan;
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            IsOptional = Left.IsOptional;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        public const int LeftPosition = 0;
        public const int RightPosition = 1;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            AnySpanCandidate result = candidateFactory.CreateAnySpanCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitAnySpan(this);
        }
    }
}
