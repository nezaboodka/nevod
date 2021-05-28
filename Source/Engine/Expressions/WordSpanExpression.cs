//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Nezaboodka.Nevod
{
    internal sealed class WordSpanExpression : AnySpanExpression
    {
        public Expression Span { get; }
        public Expression Exclusion { get; }

        public WordSpanExpression(Syntax syntax, Expression left, Expression right, Range spanRange, Expression span,
            Expression exclusion, Expression extractionOfSpan)
            : base(syntax, left, right, spanRange, extractionOfSpan)
        {
            Span = span;
            Span.SetParentExpression(this, SpanPosition);
            Exclusion = exclusion;
            if (Exclusion != null)
                Exclusion.SetParentExpression(this, ExclusionPosition);
        }

        public const int SpanPosition = 2;
        public const int ExclusionPosition = 3;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            WordSpanCandidate result = candidateFactory.CreateWordSpanCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitWordSpan(this);
        }
    }
}
