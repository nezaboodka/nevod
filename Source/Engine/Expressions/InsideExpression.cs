//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal sealed class InsideExpression : CompoundExpression
    {
        public Expression Body { get; }
        public PatternReferenceExpression OuterPattern { get; }

        public InsideExpression(Syntax syntax, Expression body, PatternReferenceExpression outerPattern)
            : base(syntax)
        {
            Body = body;
            Body.SetParentExpression(this, BodyPosition);
            OuterPattern = outerPattern;
            OuterPattern.SetParentExpression(this, OuterPatternPosition);
            RefreshIsOptional();
        }

        public override void RefreshIsOptional()
        {
            IsOptional = Body.IsOptional;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        public const int BodyPosition = 0;
        public const int OuterPatternPosition = 1;

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            var result = candidateFactory.CreateInsideCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitInside(this);
        }
    }
}
