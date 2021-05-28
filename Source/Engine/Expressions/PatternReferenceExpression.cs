//--------------------------------------------------------------------------------------------------
// Copyright © Nezaboodka™ Software LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0.
//--------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;

namespace Nezaboodka.Nevod
{
    internal sealed class PatternReferenceExpression : CompoundExpression
    {
        public PatternExpression ReferencedPattern { get; internal set; }
        public int PatternId { get; set; }

        public PatternReferenceExpression(Syntax syntax)
            : base(syntax)
        {
        }

        public override void RefreshIsOptional()
        {
            if (ReferencedPattern != null)
                IsOptional = ReferencedPattern.IsOptional;
            if (IsOptional && ParentExpression != null)
                ParentExpression.RefreshIsOptional();
        }

        // Internal

        protected override Candidate CreateCandidate(CandidateFactory candidateFactory)
        {
            PatternReferenceCandidate result = candidateFactory.CreatePatternReferenceCandidate(this);
            return result;
        }

        protected internal override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitPatternReference(this);
        }
    }
}
